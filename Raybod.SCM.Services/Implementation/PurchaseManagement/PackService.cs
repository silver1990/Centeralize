using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.Packing;
using Raybod.SCM.DataTransferObject.PO;
using Raybod.SCM.DataTransferObject.QualityControl;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Common;
using Raybod.SCM.Utility.EnumType;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class PackService : IPackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IContractFormConfigService _formConfigService;
        private readonly DbSet<PO> _poRepository;
        private readonly DbSet<POSubject> _poSubjectRepository;
        private readonly DbSet<POStatusLog> _poStatusLogRepository;
        private readonly DbSet<PAttachment> _pAttachmentRepository;
        private readonly DbSet<QualityControl> _qualityControlRepository;
        private readonly DbSet<Pack> _packRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly IPOService _poServices;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;

        public PackService(IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService authenticationServices,
            IPaymentService paymentService,
            IHttpContextAccessor httpContextAccessor,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IContractFormConfigService formConfigService,
            IPOService poServices)
        {
            _unitOfWork = unitOfWork;
            _authenticationServices = authenticationServices;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _formConfigService = formConfigService;
            _poRepository = _unitOfWork.Set<PO>();
            _poStatusLogRepository = _unitOfWork.Set<POStatusLog>();
            _qualityControlRepository = _unitOfWork.Set<QualityControl>();
            _poSubjectRepository = _unitOfWork.Set<POSubject>();
            _pAttachmentRepository = _unitOfWork.Set<PAttachment>();
            _packRepository = _unitOfWork.Set<Pack>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _poServices = poServices;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<ServiceResult<List<WaitingPOSubjectDto>>> GetWaitingPoSubjectForAddPackingAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationServices
               .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<WaitingPOSubjectDto>>(null, MessageId.AccessDenied);

                var dbQuery = _poSubjectRepository.Where(a => a.POId == poId && a.PO.BaseContractCode == authenticate.ContractCode && a.RemainedQuantity > 0)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId));

                var result = await dbQuery
                    .Select(c => new WaitingPOSubjectDto
                    {
                        ProductId = c.ProductId,
                        ProductCode = c.Product.ProductCode,
                        ProductName = c.Product.Description,
                        Quantity = c.RemainedQuantity,
                        RemainedQuantity = c.RemainedQuantity
                    }).ToListAsync();
                List<WaitingPOSubjectDto> pOSubjectDtos = new List<WaitingPOSubjectDto>();
                foreach (var item in result)
                {
                    if (!pOSubjectDtos.Any(a => a.ProductId == item.ProductId))
                    {
                        pOSubjectDtos.Add(new WaitingPOSubjectDto
                        {
                            ProductId = item.ProductId,
                            ProductCode = item.ProductCode,
                            ProductName = item.ProductName,
                            Quantity = result.Where(a => a.ProductId == item.ProductId).Sum(a => a.Quantity),
                            RemainedQuantity = result.Where(a => a.ProductId == item.ProductId).Sum(a => a.RemainedQuantity),
                        });
                    }
                }

                return ServiceResultFactory.CreateSuccess(pOSubjectDtos);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<WaitingPOSubjectDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<AddPackingResultDto>> AddPackAsync(AuthenticateDto authenticate, long poId, AddPackDto model)
        {
            try
            {
                var permission = await _authenticationServices
                .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<AddPackingResultDto>(null, MessageId.AccessDenied);

                var dbQuery = _poRepository
                  .Where(a => !a.IsDeleted && a.POId == poId && a.BaseContractCode == authenticate.ContractCode)
                  .Include(a => a.Supplier)
                  .Include(a => a.POStatusLogs)
                  .Include(a => a.POSubjects)
                  .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                    return ServiceResultFactory.CreateError<AddPackingResultDto>(null, MessageId.AccessDenied);

                var poModel = await dbQuery
                  .FirstOrDefaultAsync();

                if (poModel == null || poModel.POSubjects == null)
                    return ServiceResultFactory.CreateError<AddPackingResultDto>(null, MessageId.EntityDoesNotExist);

                if (poModel.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError<AddPackingResultDto>(null, MessageId.CantDoneBecausePOCanceled);

                if (model == null || model.PackSubjects == null || model.PackSubjects.Count() == 0)
                    return ServiceResultFactory.CreateError<AddPackingResultDto>(null, MessageId.InputDataValidationError);

                var packModel = new Pack
                {
                    PackStatus = PackStatus.Register,
                    POId = poId,
                    PackSubjects = new List<PackSubject>()
                };
                decimal neededQuantity = 0;
                foreach (var packSubject in model.PackSubjects)
                {
                    var selectedPOSubject = poModel.POSubjects
                         .Where(a => a.RemainedQuantity > 0 && a.ProductId == packSubject.ProductId)
                         .ToList();

                    if (selectedPOSubject == null)
                        return ServiceResultFactory.CreateError<AddPackingResultDto>(null, MessageId.DataInconsistency);
                    neededQuantity = packSubject.Quantity;

                    if (packSubject.Quantity <= 0)
                        continue;
                    if (packSubject.Quantity > selectedPOSubject.Sum(a => a.RemainedQuantity))
                        return ServiceResultFactory.CreateError<AddPackingResultDto>(null, MessageId.InputDataValidationError);
                    packModel.PackSubjects.Add(new PackSubject
                    {
                        ProductId = packSubject.ProductId,
                        Quantity = packSubject.Quantity,
                    });
                    foreach (var item in selectedPOSubject)
                    {
                        if (neededQuantity > 0)
                        {

                            item.RemainedQuantity -= (neededQuantity >= item.Quantity - item.ReceiptedQuantity) ? item.Quantity - item.ReceiptedQuantity : neededQuantity;
                            neededQuantity -= item.Quantity - item.ReceiptedQuantity;
                        }


                    }


                }

                if (!packModel.PackSubjects.Any())
                    return ServiceResultFactory.CreateError<AddPackingResultDto>(null, MessageId.InputDataValidationError);

                // add attachment
                if (model.Attachments != null && model.Attachments.Any())
                {
                    if (!_fileHelper.FileExistInTemp(model.Attachments.Select(a => a.FileSrc).ToList()))
                        return ServiceResultFactory.CreateError<AddPackingResultDto>(null, MessageId.FileNotFound);
                    var attachmentResult = await AddPackAttachmentAsync(packModel, model.Attachments);
                    if (!attachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError<AddPackingResultDto>(null, attachmentResult.Messages.FirstOrDefault().Message);

                    packModel = attachmentResult.Result;
                }

                if (!poModel.POStatusLogs.Any(a => a.Status == POStatus.packing))
                {
                    var poStatusLogModel = new POStatusLog
                    {
                        IsDone = false,
                        POId = poId,
                        BeforeStatus = poModel.POStatus,
                        Status = POStatus.packing
                    };
                    _poStatusLogRepository.Add(poStatusLogModel);
                }

               
                packModel.PackCode = "";

                _packRepository.Add(packModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = poModel.BaseContractCode,
                        Quantity = poModel.POCode,
                        Temp = poModel.Supplier.Name,
                        FormCode = packModel.PackCode,
                        Description = poModel.Supplier.Name,
                        KeyValue = packModel.PackId.ToString(),
                        ProductGroupId = poModel.ProductGroupId,
                        NotifEvent = NotifEvent.AddPack,
                        RootKeyValue = poModel.POId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    },
                    poModel.ProductGroupId,
                    new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent= NotifEvent.AddPackQC,
                        Roles= new List<string>
                        {
                           SCMRole.PackQCMng,
                        }
                    }
                    });
                    AddPackingResultDto result = new AddPackingResultDto();
                    var poSubjects = await _poServices.GetPODetailsByPOIdAsync(authenticate, poId);
                    if (poSubjects.Succeeded)
                        result.POSubjects = poSubjects.Result.POSubjects;
                    result.PackInfo = new PackListDto
                    {
                        DateCreated = packModel.CreatedDate.ToUnixTimestamp(),
                        PackCode = packModel.PackCode,
                        PackId = packModel.PackId,
                        PackStatus = packModel.PackStatus,
                        UserAudit = new UserAuditLogDto
                        {
                            CreateDate = packModel.CreatedDate.ToUnixTimestamp(),
                            AdderUserName = authenticate.UserFullName,
                            AdderUserImage = !String.IsNullOrEmpty(authenticate.UserImage) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + authenticate.UserImage : ""
                        }
                    };
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<AddPackingResultDto>(null, MessageId.AddEntityFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<AddPackingResultDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<PackListDto>>> GetPackListAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationServices
               .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PackListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _packRepository
                    .AsNoTracking()
                   .Where(a => !a.IsDeleted && poId == a.POId && a.PO.BaseContractCode == authenticate.ContractCode)
                  .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId));

                var result = await dbQuery.Select(c => new PackListDto
                {
                    PackId = c.PackId,
                    PackCode = c.PackCode,
                    PackStatus = c.PackStatus,
                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserName = c.AdderUser.FullName,
                        AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall : ""
                    } : null,
                    DateCreated = c.CreatedDate.ToUnixTimestamp()
                }).ToListAsync();
                List<PackListDto> orderResult = new List<PackListDto>();
                orderResult.AddRange(result.Where(a => a.PackStatus == PackStatus.Register).OrderBy(a => a.DateCreated).ToList());
                orderResult.AddRange(result.Where(a => a.PackStatus > PackStatus.RejectQC).OrderBy(a=>a.PackCode,new CompareFormNumbers()).ToList());
                orderResult.AddRange(result.Where(a => a.PackStatus == PackStatus.RejectQC).OrderBy(a => a.DateCreated).ToList());
                return ServiceResultFactory.CreateSuccess(orderResult);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PackListDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<PackListDto>> GetPackByIdForQcResultAsync(AuthenticateDto authenticate, long packId)
        {
            try
            {
                var permission = await _authenticationServices
               .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PackListDto>(null, MessageId.AccessDenied);

                var dbQuery = _packRepository
                    .AsNoTracking()
                   .Where(a => !a.IsDeleted && a.PackId == packId && a.PO.BaseContractCode == authenticate.ContractCode)
                  .AsQueryable();

                if(await dbQuery.CountAsync()==0)
                    return ServiceResultFactory.CreateError<PackListDto>(null, MessageId.EntityDoesNotExist);
                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId));
                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError<PackListDto>(null, MessageId.AccessDenied);
                var result = await dbQuery.Select(c => new PackListDto
                {
                    PackId = c.PackId,
                    PackCode = c.PackCode,
                    PackStatus = c.PackStatus,
                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserName = c.AdderUser.FullName,
                        AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall : ""
                    } : null,
                    DateCreated = c.CreatedDate.ToUnixTimestamp()
                }).FirstOrDefaultAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PackListDto>(null, exception);
            }
        }
        public async Task<ServiceResult<PackDetailsDto>> GetPackByIdAsync(AuthenticateDto authenticate, long poId, long packId)
        {
            try
            {
                var permission = await _authenticationServices
                    .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PackDetailsDto>(null, MessageId.AccessDenied);

                var dbQuery = _packRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.POId == poId && a.PackId == packId && a.PO.BaseContractCode == authenticate.ContractCode)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PackDetailsDto>(null, MessageId.AccessDenied);

                var result = await dbQuery.Select(a => new PackDetailsDto
                {
                    PackSubjects = a.PackSubjects.Select(c => new WaitingPOSubjectDto
                    {
                        ProductId = c.ProductId,
                        ProductCode = c.Product.ProductCode,
                        ProductName = c.Product.Description,
                        ProductUnit = c.Product.Unit,
                        TechnicalNumber = c.Product.TechnicalNumber,
                        Quantity = c.Quantity,

                    }).ToList(),
                    Attachments = a.PackAttachments.Where(b => !b.IsDeleted)
                                 .Select(b => new PackingAttachmentsDto
                                 {
                                     Id = b.Id,
                                     FileType = b.FileType,
                                     FileName = b.FileName,
                                     FileSize = b.FileSize,
                                     FileSrc = b.FileSrc,
                                     PackId = b.PackId.Value
                                 }).ToList(),
                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PackDetailsDto>(null, exception);
            }
        }

        public async Task<ServiceResult<PackDetailsDto>> GetPackByIdForEditAsync(AuthenticateDto authenticate, long poId, long packId)
        {
            try
            {
                var permission = await _authenticationServices
                       .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PackDetailsDto>(null, MessageId.AccessDenied);

                var dbQuery = _packRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.POId == poId && a.PackId == packId && a.PO.BaseContractCode == authenticate.ContractCode)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PackDetailsDto>(null, MessageId.AccessDenied);

                var result = await dbQuery.Select(a => new PackDetailsDto
                {
                    PackSubjects = a.PackSubjects.Select(c => new WaitingPOSubjectDto
                    {
                        ProductId = c.ProductId,
                        ProductCode = c.Product.ProductCode,
                        ProductName = c.Product.Description,
                        Quantity = c.Quantity,
                        RemainedQuantity = c.Quantity + c.Pack.PO.POSubjects.Where(a => a.ProductId == c.ProductId).Sum(a => a.RemainedQuantity),

                    }).ToList(),
                    Attachments = a.PackAttachments.Where(b => !b.IsDeleted)
                                 .Select(b => new PackingAttachmentsDto
                                 {
                                     Id = b.Id,
                                     FileType = b.FileType,
                                     FileName = b.FileName,
                                     FileSize = b.FileSize,
                                     FileSrc = b.FileSrc,
                                     PackId = b.PackId.Value
                                 }).ToList(),
                }).FirstOrDefaultAsync();
                List<WaitingPOSubjectDto> waitingPOSubjectDtos = new List<WaitingPOSubjectDto>();
                foreach (var item in result.PackSubjects)
                {
                    if (!waitingPOSubjectDtos.Any(a => a.ProductId == item.ProductId))
                    {
                        waitingPOSubjectDtos.Add(new WaitingPOSubjectDto
                        {
                            ProductId = item.ProductId,
                            ProductCode = item.ProductCode,
                            ProductName = item.ProductName,
                            Quantity = result.PackSubjects.Where(a => a.ProductId == item.ProductId).Sum(a => a.Quantity),
                            RemainedQuantity = item.RemainedQuantity,
                        });
                    }
                }
                result.PackSubjects = waitingPOSubjectDtos;
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PackDetailsDto>(null, exception);
            }
        }

        public async Task<ServiceResult<PackingQualityControlInfodto>> GetQualityControlByPackIdAsync(AuthenticateDto authenticate, long poId, long PackId)
        {
            try
            {
                var permission = await _authenticationServices
                      .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PackingQualityControlInfodto>(null, MessageId.AccessDenied);

                var dbQuery = _qualityControlRepository
                .AsNoTracking()
                .Where(a => !a.IsDeleted && a.PackId == PackId && a.Pack.POId == poId && a.Pack.PO.BaseContractCode == authenticate.ContractCode)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Pack.PO.ProductGroupId));

                PackingQualityControlInfodto result = await GetPackQCAsync(dbQuery);
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PackingQualityControlInfodto>(null, exception);
            }
        }

        private async Task<PackingQualityControlInfodto> GetPackQCAsync(IQueryable<QualityControl> dbQuery)
        {
            return await dbQuery
                .Select(c => new PackingQualityControlInfodto
                {
                    Id = c.Id,
                    PackId = c.PackId.Value,
                    Note = c.Note,
                    QCResult = c.QCResult,
                    UserAudit = c.AdderUser != null
                    ? new UserAuditLogDto
                    {
                        AdderUserId = c.AdderUserId,
                        AdderUserName = c.AdderUser.FullName,
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall +
                                         c.AdderUser.Image
                    }
                    : null,
                    Attachments = c.QCAttachments.Where(s => !s.IsDeleted)
                    .Select(v => new BaseQualityControlAttachmentDto
                    {
                        Id = v.Id,
                        FileType = v.FileType,
                        FileName = v.FileName,
                        FileSize = v.FileSize,
                        FileSrc = v.FileSrc,
                        QualityControlId = v.QualityControlId.Value
                    }).ToList(),
                }).FirstOrDefaultAsync();
        }

        public async Task<ServiceResult<PackQcResultDto>> AddQulityControlAsync(AuthenticateDto authenticate, long poId, long packId, AddQualityControlDto model)
        {
            try
            {
                var permission = await _authenticationServices
                      .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PackQcResultDto>(null, MessageId.AccessDenied);

                var dbQuery = _packRepository
                    .Where(a => a.PackId == packId && poId == a.POId && !a.IsDeleted && a.PO.BaseContractCode == authenticate.ContractCode)
                    .Include(a => a.PackSubjects)
                    .Include(a => a.PO)
                    .ThenInclude(c => c.Supplier)
                    .Include(a => a.PO)
                    .ThenInclude(c => c.POStatusLogs)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.POSubjects)
                    .AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<PackQcResultDto>(null, MessageId.EntityDoesNotExist);


                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PackQcResultDto>(null, MessageId.AccessDenied);

                var packModel = await dbQuery.FirstOrDefaultAsync();

                if (packModel == null)
                    return ServiceResultFactory.CreateError<PackQcResultDto>(null, MessageId.EntityDoesNotExist);

                if (packModel.PO.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError<PackQcResultDto>(null, MessageId.CantDoneBecausePOCanceled);

                if (packModel.PackStatus != PackStatus.Register)
                    return ServiceResultFactory.CreateError<PackQcResultDto>(null, MessageId.ImpossibleAddQC);

                packModel.PackStatus = model.QCResult == QCResult.Accept ? PackStatus.AcceptQC : PackStatus.RejectQC;
                var qualityControlModel = new QualityControl
                {
                    Note = model.Note,
                    QCResult = model.QCResult,
                    PackId = packId,
                    IsDeleted = false,
                    QCAttachments = new List<PAttachment>()
                };

                if (model.Attachments != null && model.Attachments.Any())
                {
                    if (!_fileHelper.FileExistInTemp(model.Attachments.Select(c => c.FileSrc).ToList()))
                        return ServiceResultFactory.CreateError<PackQcResultDto>(null, MessageId.FileNotFound);
                    var attachmentResult = await AddQualityControlAttachmentAsync(qualityControlModel, model.Attachments);
                    if (!attachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError<PackQcResultDto>(null,
                            attachmentResult.Messages.FirstOrDefault().Message);

                    qualityControlModel = attachmentResult.Result;
                }

                if (packModel.PackStatus == PackStatus.RejectQC)
                {
                    var poSubject = packModel.PO.POSubjects;
                    foreach (var packSubject in packModel.PackSubjects)
                    {

                        var selectedPOSubject = poSubject.Where(a => a.ProductId == packSubject.ProductId).ToList();

                        if (selectedPOSubject == null)
                            return ServiceResultFactory.CreateError<PackQcResultDto>(null, MessageId.DataInconsistency);

                        var diffrence = packSubject.Quantity;
                        decimal neededQuantity = Math.Abs(diffrence);



                        foreach (var item in selectedPOSubject.Where(a => a.ProductId == packSubject.ProductId).OrderByDescending(a => a.POSubjectId))
                        {
                            if (item.RemainedQuantity == item.Quantity)
                                continue;
                            if (item.RemainedQuantity + neededQuantity <= item.Quantity)
                            {
                                item.RemainedQuantity += neededQuantity;
                                neededQuantity = 0;
                            }

                            else
                            {
                                neededQuantity -= (item.Quantity - item.RemainedQuantity);
                                item.RemainedQuantity = item.Quantity;
                            }

                        }

                    }
                    if (!packModel.PO.POSubjects.Any(a => a.RemainedQuantity != a.Quantity))
                    {
                        var selectedLog = packModel.PO.POStatusLogs.First(a => a.Status == POStatus.packing);
                        packModel.PO.POStatus = POStatus.Approved;
                        _poStatusLogRepository.Remove(selectedLog);
                    }
                }
                else
                {
                    string counter = "0";
                    var packs = await _packRepository
                   .Where(a =>!a.IsDeleted &&a.PackStatus>=PackStatus.AcceptQC && a.POId==packModel.POId).ToListAsync();

                    if (packs != null && packs.Any())
                    {
                        var lastPack = packs.OrderByDescending(a => a.PackCode, new CompareFormNumbers()).FirstOrDefault();
                        if (lastPack == null)
                            counter = "0";
                        else
                        {
                            var codeSplit = lastPack.PackCode.Split('-');
                            counter = codeSplit[codeSplit.Length - 1];
                        }
                    }

                    else
                    {
                        counter = "0";
                    }

                   
                    packModel.PackCode =packModel.PO.POCode+"-PA-"+ (Convert.ToInt32(counter)+1).ToString();

                    packModel.Logistics = new List<Logistic>();

                    if (packModel.PO.PContractType == PContractType.Internal)
                    {
                        packModel.Logistics.Add(new Logistic
                        {
                            PackId = packId,
                            LogisticStatus = LogisticStatus.Pending,
                            Step = LogisticStep.T3
                        });
                    }
                    else
                    {
                        var packStatuses = new List<LogisticStep>
                        {
                            LogisticStep.T1,
                            LogisticStep.C1,
                            LogisticStep.T2,
                            LogisticStep.C2,
                            LogisticStep.T3
                        };

                        foreach (var item in packStatuses)
                        {
                            packModel.Logistics.Add(new Logistic
                            {
                                PackId = packId,
                                LogisticStatus = LogisticStatus.Pending,
                                Step = item
                            });
                        }
                    }

                    if (!packModel.PO.POSubjects.Any(a => a.RemainedQuantity > 0))
                    {
                        var selectedLog = packModel.PO.POStatusLogs.First(a => a.Status == POStatus.packing);
                        selectedLog.IsDone = true;
                        packModel.PO.POStatus = POStatus.packing;
                    }
                }
                _qualityControlRepository.Add(qualityControlModel);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = await GetPackQCAsync(_qualityControlRepository.Where(a => a.PackId == packId));
                    PackQcResultDto result = new PackQcResultDto();
                    result.PackStatus = packModel.PackStatus;
                    result.PackCode = packModel.PackCode;
                    var qualityInfo = await GetQualityControlByPackIdAsync(authenticate, poId, packId);
                    if (qualityInfo.Succeeded)
                        result.QualityControlInfo = qualityInfo.Result;
                    else
                        result.QualityControlInfo = new PackingQualityControlInfodto();
                    await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, packModel.PO.BaseContractCode, packModel.PackId.ToString(), NotifEvent.AddPackQC);
                    await SendNotificationAndTaskOnQcPackAsync(authenticate, model, packModel);

                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<PackQcResultDto>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PackQcResultDto>(null, exception);
            }
        }

        private async Task SendNotificationAndTaskOnQcPackAsync(AuthenticateDto authenticate, AddQualityControlDto model, Pack packModel)
        {
            var notifEvent = packModel.PO.PContractType == PContractType.Internal ? NotifEvent.T3Pending : NotifEvent.AcceptPackQC;
            var task = new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent= notifEvent,
                        Roles= new List<string>
                        {
                           SCMRole.LogisticMng
                        }
                    }
                    };

            if (model.QCResult != QCResult.Accept)
                task = null;

            await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
            {
                NotifEvent = model.QCResult == QCResult.Accept ? NotifEvent.AcceptPackQC : NotifEvent.RejectPackQC,
                Description = packModel.PO.POCode,
                Temp = packModel.PO.Supplier.Name,
                RootKeyValue = packModel.PO.POId.ToString(),
                ContractCode = packModel.PO.BaseContractCode,
                ProductGroupId = packModel.PO.ProductGroupId,
                FormCode = packModel.PackCode,
                KeyValue = packModel.PackId.ToString(),
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName
            },
            packModel.PO.ProductGroupId,
            task);
        }

        public async Task<ServiceResult<List<POSubjectInfoDto>>> DeletePackAsync(AuthenticateDto authenticate, long poId, long packId)
        {
            try
            {
                var permission = await _authenticationServices
                     .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<POSubjectInfoDto>>(null, MessageId.AccessDenied);

                var dbQuery = _packRepository
                    .Where(a => a.PackId == packId && poId == a.POId && !a.IsDeleted && a.PO.BaseContractCode == authenticate.ContractCode)
                    .Include(a => a.PackSubjects)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.POSubjects)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.POStatusLogs)
                    .AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<POSubjectInfoDto>>(null, MessageId.EntityDoesNotExist);


                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<POSubjectInfoDto>>(null, MessageId.AccessDenied);

                var packModel = await dbQuery.FirstOrDefaultAsync();

                if (packModel == null)
                    return ServiceResultFactory.CreateError<List<POSubjectInfoDto>>(null, MessageId.EntityDoesNotExist);

                if (packModel.PO.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError<List<POSubjectInfoDto>>(null, MessageId.CantDoneBecausePOCanceled);

                if (packModel.PackStatus >= PackStatus.AcceptQC || packModel.PackStatus == PackStatus.RejectQC)
                    return ServiceResultFactory.CreateError<List<POSubjectInfoDto>>(null, MessageId.ImpossibleEdit);

                var poSubject = packModel.PO.POSubjects;
                foreach (var packSubject in packModel.PackSubjects)
                {
                    var selectedPOSubject = poSubject.Where(a => a.ProductId == packSubject.ProductId).ToList();

                    if (selectedPOSubject == null)
                        return ServiceResultFactory.CreateError<List<POSubjectInfoDto>>(null, MessageId.DataInconsistency);


                    decimal neededQuantity = packSubject.Quantity;


                    foreach (var item in selectedPOSubject.Where(a => a.ProductId == packSubject.ProductId).OrderByDescending(a => a.POSubjectId))
                    {
                        if (item.RemainedQuantity == item.Quantity)
                            continue;
                        if (item.RemainedQuantity + neededQuantity <= item.Quantity)
                        {
                            item.RemainedQuantity += neededQuantity;
                            neededQuantity = 0;
                        }

                        else
                        {
                            neededQuantity -= (item.Quantity - item.RemainedQuantity);
                            item.RemainedQuantity = item.Quantity;
                        }

                    }



                }
                if (!packModel.PO.POSubjects.Any(a => a.RemainedQuantity != a.Quantity))
                {
                    var selectedLog = packModel.PO.POStatusLogs.First(a => a.Status == POStatus.packing);
                    packModel.PO.POStatus = POStatus.Approved;
                    _poStatusLogRepository.Remove(selectedLog);
                }

                packModel.IsDeleted = true;
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = await _poServices.GetPODetailsByPOIdAsync(authenticate, poId);
                    if (result.Succeeded)
                    {
                        return ServiceResultFactory.CreateSuccess(result.Result.POSubjects);
                    }
                    return ServiceResultFactory.CreateSuccess(new List<POSubjectInfoDto>());
                }
                return ServiceResultFactory.CreateError<List<POSubjectInfoDto>>(null, MessageId.DeleteEntityFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<POSubjectInfoDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> EditPackAsync(AuthenticateDto authenticate, long poId, long packId, List<AddPackSubjectDto> model)
        {
            try
            {
                var permission = await _authenticationServices
                    .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _packRepository
                     .Where(a => a.PackId == packId && poId == a.POId && a.PO.BaseContractCode == authenticate.ContractCode && !a.IsDeleted)
                     .Include(a => a.PackSubjects)
                     .Include(a => a.PO)
                     .ThenInclude(a => a.POSubjects)
                     .AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var packModel = await dbQuery.FirstOrDefaultAsync();

                if (packModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (packModel.PackStatus != PackStatus.Register)
                    return ServiceResultFactory.CreateError(false, MessageId.ImpossibleEdit);

                var poSubject = packModel.PO.POSubjects;
                foreach (var packSubject in packModel.PackSubjects)
                {




                    var selectedPOSubject = poSubject.Where(a => a.ProductId == packSubject.ProductId).ToList();
                    if (selectedPOSubject == null)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                    var postedSubject = model.FirstOrDefault(a => a.ProductId == packSubject.ProductId);
                    var diffrence = packSubject.Quantity - postedSubject.Quantity;
                    decimal neededQuantity = Math.Abs(diffrence);
                    if (postedSubject == null)
                        return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);





                    if (diffrence > 0)
                    {

                        foreach (var item in selectedPOSubject.Where(a => a.ProductId == packSubject.ProductId).OrderByDescending(a => a.POSubjectId))
                        {
                            if (item.RemainedQuantity == item.Quantity)
                                continue;
                            if (item.RemainedQuantity + neededQuantity <= item.Quantity)
                            {
                                item.RemainedQuantity += neededQuantity;
                                neededQuantity = 0;
                            }

                            else
                            {
                                neededQuantity -= (item.Quantity - item.RemainedQuantity);
                                item.RemainedQuantity = item.Quantity;
                            }

                        }
                    }
                    else if (diffrence < 0)
                    {
                        if (diffrence > selectedPOSubject.Where(a => a.ProductId == packSubject.ProductId).Sum(a => a.ReceiptedQuantity))
                            return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);
                        foreach (var item in selectedPOSubject.Where(a => a.ProductId == packSubject.ProductId).OrderBy(a => a.POSubjectId))
                        {
                            if (item.RemainedQuantity == 0)
                                continue;
                            if (item.RemainedQuantity >= neededQuantity)
                            {
                                item.RemainedQuantity -= neededQuantity;
                                neededQuantity = 0;
                            }

                            else
                            {
                                neededQuantity -= item.RemainedQuantity;
                                item.RemainedQuantity = 0;
                            }
                        }
                    }

                    packSubject.Quantity = postedSubject.Quantity;

                }



                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    // todo: notification
                }
                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeletePackAttachmentAsync(AuthenticateDto authenticate, long poId, long packId, string fileName)
        {
            try
            {
                var permission = await _authenticationServices
                  .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _packRepository
                    .Where(a => !a.IsDeleted && a.PackId == packId && a.POId == poId &&
                    a.PO.BaseContractCode == authenticate.ContractCode &&
                    a.PackAttachments.Any(c => !c.IsDeleted && c.FileName == fileName))
                    .Include(a => a.PO)
                    .Include(a => a.PackAttachments)
                     .AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);


                var packModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (packModel == null || packModel.PackAttachments == null || packModel.PackAttachments.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);


                if (packModel.PO.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError(false, MessageId.CantDoneBecausePOCanceled);

                var attachModel = packModel.PackAttachments.FirstOrDefault(a => a.FileName == fileName);
                if (attachModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                attachModel.IsDeleted = true;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {

                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.DeleteEntityFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<PackingAttachmentsDto>> AddPackAttachmentAsync(AuthenticateDto authenticate, long poId, long packId, AddAttachmentDto file)
        {
            try
            {
                var permission = await _authenticationServices
                .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<PackingAttachmentsDto>(null, MessageId.AccessDenied);

                var dbQuery = _packRepository
                    .Where(a => !a.IsDeleted && a.PackId == packId && a.POId == poId && a.PO.BaseContractCode == authenticate.ContractCode)
                    .Include(a => a.PO)
                     .AsQueryable();

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<PackingAttachmentsDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<PackingAttachmentsDto>(null, MessageId.AccessDenied);

                var packModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (packModel == null)
                    return ServiceResultFactory.CreateError<PackingAttachmentsDto>(null, MessageId.EntityDoesNotExist);

                if (packModel.PO.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError<PackingAttachmentsDto>(null, MessageId.CantDoneBecausePOCanceled);

                if (string.IsNullOrEmpty(file.FileSrc) || string.IsNullOrEmpty(file.FileName))
                    return ServiceResultFactory.CreateError<PackingAttachmentsDto>(null, MessageId.InputDataValidationError);

                var UploadedFile =
                     await _fileHelper.SaveDocumentFromTemp(file.FileSrc, ServiceSetting.UploadFilePath.PO);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<PackingAttachmentsDto>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(file.FileSrc);
                var attachmentModel = new PAttachment
                {
                    PackId = packId,
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = file.FileName,
                    FileSrc = file.FileSrc,
                };

                _pAttachmentRepository.Add(attachmentModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new PackingAttachmentsDto
                    {
                        Id = attachmentModel.Id,
                        PackId = attachmentModel.PackId.Value,
                        FileName = attachmentModel.FileName,
                        FileSrc = attachmentModel.FileSrc,
                        FileSize = attachmentModel.FileSize,
                        FileType = attachmentModel.FileType
                    };
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<PackingAttachmentsDto>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<PackingAttachmentsDto>(null, exception);
            }
        }

        private async Task<ServiceResult<QualityControl>> AddQualityControlAttachmentAsync(QualityControl qualityControl, List<AddAttachmentDto> attachment)
        {
            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.PO);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<QualityControl>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                qualityControl.QCAttachments.Add(new PAttachment
                {
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc,
                });
            }

            return ServiceResultFactory.CreateSuccess(qualityControl);
        }

        private async Task<ServiceResult<Pack>> AddPackAttachmentAsync(Pack pack, List<AddAttachmentDto> attachment)
        {
            pack.PackAttachments = new List<PAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.PO);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<Pack>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                pack.PackAttachments.Add(new PAttachment
                {
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc
                });
            }

            return ServiceResultFactory.CreateSuccess(pack);
        }

        public async Task<DownloadFileDto> DownloadPackAttachmentAsync(AuthenticateDto authenticate, long poId, long packId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationServices
                    .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _packRepository
                   .Where(a => a.PackId == packId &&
                   poId == a.POId &&
                   !a.IsDeleted &&
                   a.PO.BaseContractCode == authenticate.ContractCode &&
                   a.PackAttachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc))
                   .AsQueryable();

                if (dbQuery.Count() == 0)
                    return null;

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return null;

                var streamResult = await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.FileSection.PO);
                if (streamResult == null)
                    return null;

                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<DownloadFileDto> DownloadPackQualityControlAttachmentAsync(AuthenticateDto authenticate, long poId, long packId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationServices
               .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _qualityControlRepository
                    .Where(a => !a.IsDeleted && a.PackId != null && a.PackId == packId &&
                    a.Pack.POId == poId &&
                    a.Pack.PO.BaseContractCode == authenticate.ContractCode &&
                    a.QCAttachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc))
                     .AsQueryable();

                if (dbQuery.Count() == 0)
                    return null;

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.Pack.PO.ProductGroupId)))
                    return null;

                var streamResult = await _fileHelper.DownloadDocument(fileSrc, ServiceSetting.FileSection.PO);
                if (streamResult == null)
                    return null;

                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

    }

}
