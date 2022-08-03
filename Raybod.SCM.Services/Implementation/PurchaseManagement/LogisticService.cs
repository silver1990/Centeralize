using Exon.TheWeb.Service.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Logistic;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Extention;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class LogisticService : ILogisticService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly DbSet<POStatusLog> _poStatusLogRepository;
        private readonly DbSet<PAttachment> _pAttachmentRepository;
        private readonly DbSet<Logistic> _logisticRepository;
        private readonly DbSet<Pack> _packRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly IFileService _fileService;
        private readonly CompanyAppSettingsDto _appSettings;

        public LogisticService(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService authenticationServices,
            IPaymentService paymentService,
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IFileService fileService
            )
        {
            _unitOfWork = unitOfWork;
            _authenticationServices = authenticationServices;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _fileService = fileService;
            _appSettings = appSettings.Value;
            _poStatusLogRepository = _unitOfWork.Set<POStatusLog>();
            _pAttachmentRepository = _unitOfWork.Set<PAttachment>();
            _packRepository = _unitOfWork.Set<Pack>();
            _logisticRepository = _unitOfWork.Set<Logistic>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
        }

        public async Task<ServiceResult<List<PackLogisticListDto>>> GetPoPackLogisticAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationServices
                    .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PackLogisticListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _packRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.POId == poId && a.PO.BaseContractCode == authenticate.ContractCode && a.PackStatus >= Domain.Enum.PackStatus.AcceptQC);

                if (permission.ProductGroupIds.Any())
                    dbQuery= dbQuery.Where(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId));

                var result = await dbQuery
                    .Select(c => new PackLogisticListDto
                    {
                        PackId = c.PackId,
                        PackCode = c.PackCode,
                        PackStatus = c.PackStatus,
                        UserAudit = c.AdderUser != null ? new UserAuditLogDto
                        {
                            CreateDate = c.CreatedDate.ToUnixTimestamp(),
                            AdderUserName = c.AdderUser.FullName,
                            AdderUserImage = c.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : ""
                        } : null,
                        Logistics = c.Logistics.Select(v => new BaseLogisticDto
                        {
                            LogisticId = v.LogisticId,
                            LogisticStatus = v.LogisticStatus,
                            PackId = v.PackId,
                            Step = v.Step,
                            DateStart = v.DateStart.ToUnixTimestamp(),
                            DateEnd = v.DateEnd.ToUnixTimestamp(),
                            CreaterUserAudit = v.LogisticStatus != LogisticStatus.Pending && v.AdderUser != null ? new UserAuditLogDto
                            {
                                CreateDate = v.CreatedDate.ToUnixTimestamp(),
                                AdderUserName = v.AdderUser.FullName,
                                AdderUserImage = v.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + v.AdderUser.Image : ""
                            } : null,

                            ModifierUserAudit = v.LogisticStatus == LogisticStatus.Compeleted && c.ModifierUser != null ? new UserAuditLogDto
                            {
                                CreateDate = c.UpdateDate.ToUnixTimestamp(),
                                AdderUserName = c.ModifierUser.UserName,
                                AdderUserImage = c.ModifierUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + v.AdderUser.Image : ""
                            } : null,
                        }).ToList()
                    }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PackLogisticListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<BaseLogisticDto>>> GetPackLogisticByPackIdAsync(AuthenticateDto authenticate, long poId, long packId)
        {
            try
            {
                var permission = await _authenticationServices
                    .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BaseLogisticDto>>(null, MessageId.AccessDenied);

                var dbQuery = _logisticRepository
                    .AsNoTracking()
                    .Where(a =>
                    !a.Pack.IsDeleted &&
                    a.PackId == packId &&
                    a.Pack.POId == poId &&
                    !a.Pack.PO.IsDeleted &&
                    a.Pack.PO.BaseContractCode == authenticate.ContractCode &&
                    a.Pack.PackStatus >= Domain.Enum.PackStatus.AcceptQC);
                
                //if(dbQuery.Any(a => a.Pack.PackStatus >= Domain.Enum.PackStatus.AcceptQC))
                //    return ServiceResultFactory.CreateError<List<BaseLogisticDto>>(null, MessageId.no);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.Pack.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<BaseLogisticDto>>(null, MessageId.AccessDenied);

                var result = await dbQuery
                    .Select(v => new BaseLogisticDto
                    {
                        LogisticId = v.LogisticId,
                        LogisticStatus = v.LogisticStatus,
                        PackId = v.PackId,
                        Step = v.Step,
                        DateStart = v.DateStart.ToUnixTimestamp(),
                        DateEnd = v.DateEnd.ToUnixTimestamp(),
                        CreaterUserAudit = v.LogisticStatus != LogisticStatus.Pending && v.AdderUser != null ? new UserAuditLogDto
                        {
                            CreateDate = v.CreatedDate.ToUnixTimestamp(),
                            AdderUserName = v.AdderUser.FullName,
                            AdderUserImage = v.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + v.AdderUser.Image : ""
                        } : null,

                        ModifierUserAudit = v.LogisticStatus == LogisticStatus.Compeleted && v.ModifierUser != null ? new UserAuditLogDto
                        {
                            CreateDate = v.UpdateDate.ToUnixTimestamp(),
                            AdderUserName = v.ModifierUser.UserName,
                            AdderUserImage = v.ModifierUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + v.AdderUser.Image : ""
                        } : null
                    }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<BaseLogisticDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> StartTransportationAsync(AuthenticateDto authenticate, long poId, long packId, LogisticStep logisticStep)
        {
            try
            {
                var acceptLogisticStep = new List<LogisticStep> { LogisticStep.T1, LogisticStep.T2, LogisticStep.T3 };
                if (!acceptLogisticStep.Contains(logisticStep))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var permission = await _authenticationServices
                   .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _packRepository
                    .Where(a => !a.IsDeleted && a.PackId == packId && a.POId == poId && a.PO.BaseContractCode == authenticate.ContractCode)
                    .Include(a => a.Logistics)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.POStatusLogs)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.Supplier)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var packModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (packModel == null || packModel.Logistics == null || !packModel.Logistics.Any(a => a.Step == logisticStep))
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);


                if (packModel.PO.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError(false, MessageId.CantDoneBecausePOCanceled);

                var packStatus = ReturnAcceptablePackStatusInThisLogisticStep(packModel.PO.PContractType, logisticStep, true);

                if (packModel.PackStatus != packStatus)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var selectedLogisticModel = packModel.Logistics.FirstOrDefault(a => a.Step == logisticStep);
                if (selectedLogisticModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var nextPackStatus = ReturnNextPackStatusInThisLogisticStep(packModel.PO.PContractType, packStatus.Value);
                if (nextPackStatus == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                packModel.PackStatus = nextPackStatus.Value;
                selectedLogisticModel.LogisticStatus = LogisticStatus.Inprogress;
                selectedLogisticModel.DateStart = DateTime.UtcNow;

                await UpdatePOStatusOnStratTransportationAsync(poId, packId, packModel, false);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = packModel.PO.BaseContractCode,
                        FormCode = packModel.PackCode,
                        Description = packModel.PO.POCode,
                        Temp=packModel.PO.Supplier.Name,
                        KeyValue = packModel.PackId.ToString(),
                        NotifEvent = GetNotifEventPack(packModel.PackStatus),
                        RootKeyValue = packModel.PO.POId.ToString(),
                        ProductGroupId = packModel.PO.ProductGroupId,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    }, null);
                    //new List<NotifToDto> {
                    //new NotifToDto
                    //{
                    //    NotifEvent= GetNextTaskNotifEvent(packModel.PackStatus),
                    //    Roles= new List<string>
                    //    {
                    //       SCMRole.LogisticMng,
                    //    }
                    //}
                    //}
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> CompeleteTransportationAsync(AuthenticateDto authenticate, long poId, long packId, LogisticStep logisticStep)
        {
            try
            {
                var acceptLogisticStep = new List<LogisticStep> { LogisticStep.T1, LogisticStep.T2, LogisticStep.T3 };
                if (!acceptLogisticStep.Contains(logisticStep))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var permission = await _authenticationServices
                   .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _packRepository
                    .Where(a => !a.IsDeleted && a.PackId == packId && a.POId == poId && a.PO.BaseContractCode == authenticate.ContractCode)
                    .Include(a => a.Logistics)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.POStatusLogs)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.Supplier)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var packModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (packModel == null || packModel.Logistics == null || !packModel.Logistics.Any(a => a.Step == logisticStep))
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (packModel.PO.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError(false, MessageId.CantDoneBecausePOCanceled);

                var poModel = packModel.PO;

                var packStatus = ReturnAcceptablePackStatusInThisLogisticStep(packModel.PO.PContractType, logisticStep, false);

                if (packModel.PackStatus != packStatus)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var selectedLogisticModel = packModel.Logistics.FirstOrDefault(a => a.Step == logisticStep);
                if (selectedLogisticModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var nextPackStatus = ReturnNextPackStatusInThisLogisticStep(packModel.PO.PContractType, packStatus.Value);
                if (nextPackStatus == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                packModel.PackStatus = nextPackStatus.Value;
                selectedLogisticModel.LogisticStatus = LogisticStatus.Compeleted;
                selectedLogisticModel.DateEnd = DateTime.UtcNow;
                // update po status


                await UpdatePOStatusOnStratTransportationAsync(poId, packId, packModel, true);
                if (logisticStep == LogisticStep.T3)
                    await UpdatePoStatus(authenticate.UserId, poModel, selectedLogisticModel.LogisticId);
                var taskEvent = ReturnNotificationOnEndTransportation(packModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService
                         .SetDonedNotificationAsync(authenticate.UserId, packModel.PO.BaseContractCode, packId.ToString(), GetTaskDonedNotifEventPack(packModel.PO.PContractType, packModel.PackStatus));
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = packModel.PO.BaseContractCode,
                        FormCode = packModel.PackCode,
                        Description=packModel.PO.POCode,
                        Temp = packModel.PO.Supplier.Name,
                        KeyValue = packModel.PackId.ToString(),
                        NotifEvent = GetNotifEventPack(packModel.PackStatus),
                        RootKeyValue = packModel.PO.POId.ToString(),
                        ProductGroupId = packModel.PO.ProductGroupId,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    }, taskEvent);
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private List<NotifToDto> ReturnNotificationOnEndTransportation(Pack packModel)
        {

            var notifEvent = GetNextTaskNotifEvent(packModel.PackStatus);
            var role = new List<string>();
            if (notifEvent != NotifEvent.AddReceipt)
            {
                role.Add(SCMRole.LogisticMng);
            }
            else
            {
                role.Add(SCMRole.WarehouseMng);
            }
            return new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent= notifEvent,
                        Roles= role
                    }
            };
        }

        public async Task<ServiceResult<bool>> StartClearancePortAsync(AuthenticateDto authenticate, long poId, long packId, LogisticStep logisticStep)
        {
            try
            {
                var acceptLogisticStep = new List<LogisticStep> { LogisticStep.C1, LogisticStep.C2 };
                if (!acceptLogisticStep.Contains(logisticStep))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var permission = await _authenticationServices
                  .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _packRepository
                    .Where(a => !a.IsDeleted && a.PackId == packId && a.POId == poId && a.PO.BaseContractCode == authenticate.ContractCode)
                    .Include(a => a.Logistics)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.POStatusLogs)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.Supplier)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var packModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (packModel == null || packModel.Logistics == null || !packModel.Logistics.Any(a => a.Step == logisticStep))
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (packModel.PO.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError(false, MessageId.CantDoneBecausePOCanceled);

                var packStatus = ReturnAcceptablePackStatusInThisLogisticStep(packModel.PO.PContractType, logisticStep, true);

                if (packModel.PackStatus != packStatus)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var selectedLogisticModel = packModel.Logistics.FirstOrDefault(a => a.Step == logisticStep);
                if (selectedLogisticModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var nextPackStatus = ReturnNextPackStatusInThisLogisticStep(packModel.PO.PContractType, packStatus.Value);
                if (nextPackStatus == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                packModel.PackStatus = nextPackStatus.Value;

                selectedLogisticModel.LogisticStatus = LogisticStatus.Inprogress;
                selectedLogisticModel.DateStart = DateTime.UtcNow;

                await UpdatePOStatusOnStratTransportationAsync(poId, packId, packModel, false);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = packModel.PO.BaseContractCode,
                        FormCode = packModel.PackCode,
                        Description = packModel.PO.POCode,
                        Temp = packModel.PO.Supplier.Name,
                        KeyValue = packModel.PackId.ToString(),
                        NotifEvent = GetNotifEventPack(packModel.PackStatus),
                        RootKeyValue = packModel.PO.POId.ToString(),
                        ProductGroupId = packModel.PO.ProductGroupId,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    }, null);

                    //new List<NotifToDto> {
                    //new NotifToDto
                    //{
                    //    NotifEvent= GetNextTaskNotifEvent(packModel.PackStatus),
                    //    Roles= new List<string>
                    //    {
                    //       SCMRole.LogisticMng,
                    //    }
                    //}
                    //}
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> CompeleteClearancePortAsync(AuthenticateDto authenticate, long poId, long packId, LogisticStep logisticStep)
        {
            try
            {
                var acceptLogisticStep = new List<LogisticStep> { LogisticStep.C1, LogisticStep.C2 };
                if (!acceptLogisticStep.Contains(logisticStep))
                    return ServiceResultFactory.CreateError(false, MessageId.InputDataValidationError);

                var permission = await _authenticationServices
                .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _packRepository
                    .Where(a => !a.IsDeleted && a.PackId == packId && a.POId == poId && a.PO.BaseContractCode == authenticate.ContractCode)
                    .Include(a => a.Logistics)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.POStatusLogs)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.Supplier)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);


                var packModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (packModel == null || packModel.Logistics == null || !packModel.Logistics.Any(a => a.Step == logisticStep))
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (packModel.PO.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError(false, MessageId.CantDoneBecausePOCanceled);

                var packStatus = ReturnAcceptablePackStatusInThisLogisticStep(packModel.PO.PContractType, logisticStep, false);

                if (packModel.PackStatus != packStatus)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var selectedLogisticModel = packModel.Logistics.FirstOrDefault(a => a.Step == logisticStep);
                if (selectedLogisticModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);

                var nextPackStatus = ReturnNextPackStatusInThisLogisticStep(packModel.PO.PContractType, packStatus.Value);
                if (nextPackStatus == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DataInconsistency);
                packModel.PackStatus = nextPackStatus.Value;
                selectedLogisticModel.LogisticStatus = LogisticStatus.Compeleted;
                selectedLogisticModel.DateEnd = DateTime.UtcNow;

                var taskEvent = GetNextTaskNotifEvent(packModel.PackStatus);
                await UpdatePOStatusOnStratTransportationAsync(poId, packId, packModel, true);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService
                        .SetDonedNotificationAsync(authenticate.UserId, packModel.PO.BaseContractCode, packId.ToString(), GetTaskDonedNotifEventPack(packModel.PO.PContractType, packModel.PackStatus));
                    var res = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = packModel.PO.BaseContractCode,
                        FormCode = packModel.PackCode,
                        Description = packModel.PO.POCode,
                        Temp = packModel.PO.Supplier.Name,
                        KeyValue = packModel.PackId.ToString(),
                        NotifEvent = GetNotifEventPack(packModel.PackStatus),
                        RootKeyValue = packModel.PO.POId.ToString(),
                        ProductGroupId = packModel.PO.ProductGroupId,
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName
                    },
                    packModel.PO.ProductGroupId,
                    new List<NotifToDto> {
                    new NotifToDto
                    {
                        NotifEvent= taskEvent,
                        Roles= new List<string>
                        {
                           SCMRole.LogisticMng,
                        }
                    }
                    });
                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private NotifEvent GetNextTaskNotifEvent(PackStatus status)
        {
            switch (status)
            {
                case PackStatus.C1Pending:
                    return NotifEvent.C1Pending;
                case PackStatus.T2Pending:
                    return NotifEvent.T2Pending;
                case PackStatus.C2Pending:
                    return NotifEvent.C2Pending;
                case PackStatus.T3Pending:
                    return NotifEvent.T3Pending;
                case PackStatus.PendingDelivered:
                    return NotifEvent.AddReceipt;
                default:
                    return NotifEvent.C1Pending;
            }
        }

        private NotifEvent GetNotifEventPack(PackStatus status)
        {
            switch (status)
            {
                case PackStatus.T1Inprogress:
                    return NotifEvent.T1Inprogress;
                case PackStatus.C1Pending:
                    return NotifEvent.T1Compeleted;
                case PackStatus.C1Inprogress:
                    return NotifEvent.C1Inprogress;
                case PackStatus.T2Pending:
                    return NotifEvent.C1ICompeleted;
                case PackStatus.T2Inprogress:
                    return NotifEvent.T2Inprogress;
                case PackStatus.C2Pending:
                    return NotifEvent.T2Compeleted;
                case PackStatus.C2Inprogress:
                    return NotifEvent.C2Inprogress;
                case PackStatus.T3Pending:
                    return NotifEvent.C2ICompeleted;
                case PackStatus.T3Inprogress:
                    return NotifEvent.T3Inprogress;
                case PackStatus.PendingDelivered:
                    return NotifEvent.T3Compeleted;
                default:
                    return NotifEvent.T3Compeleted;
            }
        }

        private NotifEvent GetTaskDonedNotifEventPack(PContractType pContractType, PackStatus status)
        {
            if (pContractType == PContractType.Foreign)
            {
                switch (status)
                {
                    case PackStatus.C1Pending:
                        return NotifEvent.AcceptPackQC;
                    case PackStatus.T2Pending:
                        return NotifEvent.C1Pending;
                    case PackStatus.C2Pending:
                        return NotifEvent.T2Pending;
                    case PackStatus.T3Pending:
                        return NotifEvent.C2Pending;
                    case PackStatus.PendingDelivered:
                        return NotifEvent.T3Pending;
                    default:
                        return NotifEvent.C1Pending;
                }

            }
            else
            {
                return NotifEvent.T3Pending;
            }
        }

        public async Task<ServiceResult<List<LogisticAttachmentDto>>> GetLogisticAttachmentAsync(AuthenticateDto authenticate, long poId, long packId, LogisticStep logisticStep)
        {
            try
            {
                var permission = await _authenticationServices
               .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<LogisticAttachmentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _logisticRepository
                    .Where(a => a.Step == logisticStep && a.PackId == packId && a.Pack.POId == poId && a.Pack.PO.BaseContractCode == authenticate.ContractCode)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.Pack.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<LogisticAttachmentDto>>(null, MessageId.AccessDenied);

                var logisticModel = await dbQuery
                    .Select(c => new
                    {
                        logisticId = c.LogisticId,
                        contractCode = c.Pack.PO.BaseContractCode,
                        logisticStatus = c.LogisticStatus
                    }).FirstOrDefaultAsync();

                if (logisticModel == null)
                    return ServiceResultFactory.CreateError<List<LogisticAttachmentDto>>(null, MessageId.EntityDoesNotExist);

                var result = await _pAttachmentRepository.Where(a => !a.IsDeleted && a.LogisticId == logisticModel.logisticId)
                    .Select(c => new LogisticAttachmentDto
                    {
                        Id = c.Id,
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        FileType = c.FileType
                    }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<LogisticAttachmentDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<LogisticAttachmentDto>>> AddLogisticAttachmentAsync(AuthenticateDto authenticate, long poId,
            long packId, LogisticStep logisticStep, IFormFileCollection files)
        {
            try
            {
                if (files == null || !files.Any())
                    return ServiceResultFactory.CreateError<List<LogisticAttachmentDto>>(null, MessageId.InputDataValidationError);

                var permission = await _authenticationServices
                    .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<LogisticAttachmentDto>>(null, MessageId.AccessDenied);

                var dbQuery = _logisticRepository
                    .Where(a => a.Step == logisticStep && a.PackId == packId && a.Pack.POId == poId && a.Pack.PO.BaseContractCode == authenticate.ContractCode)
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.Pack.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<LogisticAttachmentDto>>(null, MessageId.AccessDenied);

                var logisticModel = await dbQuery
                    .Select(c => new
                    {
                        logisticId = c.LogisticId,
                        contractCode = c.Pack.PO.BaseContractCode,
                        logisticStatus = c.LogisticStatus,
                        POStatus=c.Pack.PO.POStatus
                    }).FirstOrDefaultAsync();

                if (logisticModel == null)
                    return ServiceResultFactory.CreateError<List<LogisticAttachmentDto>>(null, MessageId.EntityDoesNotExist);

                if (logisticModel.POStatus == POStatus.Canceled)
                    return ServiceResultFactory.CreateError<List<LogisticAttachmentDto>>(null, MessageId.CantDoneBecausePOCanceled);
                 
                var attachModels = new List<PAttachment>();
                foreach (var item in files)
                {
                    var fileName = item.FileName;
                    var uploadResult = await _fileService.UploadDocumentFile(item);
                    if (!uploadResult.Succeeded)
                        return ServiceResultFactory.CreateError<List<LogisticAttachmentDto>>(null, uploadResult.Messages[0].Message);

                    var UploadedFile = await _fileHelper
                        .SaveDocumentFromTemp(uploadResult.Result, ServiceSetting.UploadFilePath.PO);

                    if (UploadedFile == null)
                        return ServiceResultFactory.CreateError<List<LogisticAttachmentDto>>(null, MessageId.UploudFailed);

                    _fileHelper.DeleteDocumentFromTemp(uploadResult.Result);

                    attachModels.Add(new PAttachment
                    {
                        LogisticId = logisticModel.logisticId,
                        FileSrc = uploadResult.Result,
                        FileName = fileName,
                        FileType = UploadedFile.FileType,
                        FileSize = UploadedFile.FileSize
                    });
                }

                _pAttachmentRepository.AddRange(attachModels);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var res = attachModels.Select(c => new LogisticAttachmentDto
                    {
                        Id = c.Id,
                        FileName = c.FileName,
                        FileSize = c.FileSize,
                        FileSrc = c.FileSrc,
                        FileType = c.FileType
                    }).ToList();
                    return ServiceResultFactory.CreateSuccess(res);
                }

                return ServiceResultFactory.CreateError<List<LogisticAttachmentDto>>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<LogisticAttachmentDto>>(null, exception);
            }
        }

        public async Task<DownloadFileDto> DownloadLogisticAttachmentAsync(AuthenticateDto authenticate, long poId, long packId, LogisticStep logisticStep, string fileSrc)
        {
            try
            {
                var permission = await _authenticationServices
                .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _logisticRepository
                    .Where(a => a.Step == logisticStep &&
                    a.PackId == packId &&
                    a.Pack.POId == poId &&
                    a.Pack.PO.BaseContractCode == authenticate.ContractCode &&
                    a.Attachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc))
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.Pack.PO.ProductGroupId)))
                    return null;

                var logisticModel = await dbQuery
                    .Select(c => new
                    {
                        logisticId = c.LogisticId,
                        contractCode = c.Pack.PO.BaseContractCode,
                        logisticStatus = c.LogisticStatus
                    }).FirstOrDefaultAsync();

                if (logisticModel == null)
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

        public async Task<ServiceResult<bool>> DeleteLogisticAttachmentAsync(AuthenticateDto authenticate, long poId, long packId, LogisticStep step, string fileSrc)
        {
            try
            {
                var permission = await _authenticationServices
              .HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = _logisticRepository
                    .Where(a => a.Step == step &&
                    a.PackId == packId &&
                    a.Pack.POId == poId &&
                    a.Pack.PO.BaseContractCode == authenticate.ContractCode &&
                    a.Attachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc))
                    .AsQueryable();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.Pack.PO.ProductGroupId)))
                    return null;

                var logisticModel = await dbQuery
                    .Select(c => new
                    {
                        logisticId = c.LogisticId,
                        contractCode = c.Pack.PO.BaseContractCode,
                        logisticStatus = c.LogisticStatus,
                        poStatus=c.Pack.PO.POStatus,
                        attachments = c.Attachments
                    }).FirstOrDefaultAsync();

                if (logisticModel == null || logisticModel.attachments == null)
                    return null;
                if (logisticModel.poStatus == POStatus.Canceled)
                    return ServiceResultFactory.CreateError(false,MessageId.CantDoneBecausePOCanceled);

                var attachModel = logisticModel.attachments.FirstOrDefault(a => !a.IsDeleted && a.FileSrc == fileSrc);
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

        private async Task<bool> UpdatePOStatusOnStratTransportationAsync(long poId, long packId, Pack packModel, bool endAction)
        {
            var nextPOstatus = ReturnCurrentPOStatusOnThisLogisticPackStatus(packModel.PackStatus);
            if (nextPOstatus == null)
                return false;

            if (!packModel.PO.POStatusLogs.Any(c => c.Status == nextPOstatus.Value))
            {
                var poStatusLogModel = new POStatusLog
                {
                    IsDone = false,
                    Status = nextPOstatus.Value,
                    POId = packModel.POId,
                    BeforeStatus=packModel.PO.POStatus
                };

                if (packModel.PO.POStatus >= POStatus.packing &&
                    !await _packRepository.AnyAsync(a => !a.IsDeleted && a.POId == poId && a.PackId != packId && a.PackStatus >= PackStatus.AcceptQC && a.PackStatus < packModel.PackStatus))
                {
                    packModel.PO.POStatus = nextPOstatus.Value;
                    poStatusLogModel.IsDone = true;
                }

                _poStatusLogRepository.Add(poStatusLogModel);
            }
            else if (endAction)
            {
                var statusModel = packModel.PO.POStatusLogs.First(c => c.Status == nextPOstatus);
                if (packModel.PO.POStatus >= POStatus.packing &&
                    !await _packRepository.AnyAsync(a => !a.IsDeleted && a.POId == poId && a.PackStatus >= PackStatus.AcceptQC && a.PackId != packId && a.PackStatus < packModel.PackStatus))
                {
                    packModel.PO.POStatus = nextPOstatus.Value;
                    statusModel.IsDone = true;
                }
            }
            return true;
        }

        private PackStatus? ReturnAcceptablePackStatusInThisLogisticStep(PContractType contractType, LogisticStep step, bool isWantStartStep)
        {
            if (contractType == PContractType.Foreign)
            {
                switch (step)
                {
                    case LogisticStep.T1:
                        return isWantStartStep ? PackStatus.AcceptQC : PackStatus.T1Inprogress;
                    case LogisticStep.C1:
                        return isWantStartStep ? PackStatus.C1Pending : PackStatus.C1Inprogress;
                    case LogisticStep.T2:
                        return isWantStartStep ? PackStatus.T2Pending : PackStatus.T2Inprogress;
                    case LogisticStep.C2:
                        return isWantStartStep ? PackStatus.C2Pending : PackStatus.C2Inprogress;
                    case LogisticStep.T3:
                        return isWantStartStep ? PackStatus.T3Pending : PackStatus.T3Inprogress;
                    default:
                        return null;
                }
            }
            else
            {
                switch (step)
                {
                    case LogisticStep.T3:
                        return isWantStartStep ? PackStatus.AcceptQC : PackStatus.T3Inprogress;
                    default:
                        return null;
                }
            }
        }

        private PackStatus? ReturnNextPackStatusInThisLogisticStep(PContractType contractType, PackStatus currentPackStatus)
        {
            if (contractType == PContractType.Foreign)
            {
                return (PackStatus)((int)currentPackStatus + 1);
            }
            else
            {
                switch (currentPackStatus)
                {
                    case PackStatus.AcceptQC:
                        return PackStatus.T3Inprogress;

                    case PackStatus.T3Inprogress:
                        return PackStatus.PendingDelivered;
                    default:
                        return null;
                }
            }
        }

        private POStatus? ReturnCurrentPOStatusOnThisLogisticPackStatus(PackStatus packStatus)
        {
            switch (packStatus)
            {
                case PackStatus.T1Inprogress:
                    return POStatus.TransportationToOriginPort;
                case PackStatus.C1Pending:
                    return POStatus.TransportationToOriginPort;
                case PackStatus.C1Inprogress:
                    return POStatus.OriginPort;
                case PackStatus.T2Pending:
                    return POStatus.OriginPort;
                case PackStatus.T2Inprogress:
                    return POStatus.TransportationToDestinationPort;
                case PackStatus.C2Pending:
                    return POStatus.TransportationToDestinationPort;
                case PackStatus.C2Inprogress:
                    return POStatus.DestinationPort;
                case PackStatus.T3Pending:
                    return POStatus.DestinationPort;
                case PackStatus.T3Inprogress:
                    return POStatus.TransportationToCompanyLocation;
                case PackStatus.PendingDelivered:
                    return POStatus.TransportationToCompanyLocation;

                default:
                    return null;
            }
        }

        private async Task<bool> UpdatePoStatus(int userId, PO poModel,long logisticId)
        {
            var poStatusLogs = await _poStatusLogRepository.FirstOrDefaultAsync(a => a.POId == poModel.POId && a.Status == POStatus.Delivered);
            if (poStatusLogs == null)
            {
                var newStatusLogs = new POStatusLog
                {
                    IsDone = false,
                    POId = poModel.POId,
                    BeforeStatus = poModel.POStatus,
                    Status = POStatus.Delivered,
                };

                if (poModel.POStatus >= POStatus.packing && !await _packRepository.AnyAsync(a => !a.IsDeleted && a.POId == poModel.POId &&a.Logistics.Any(a=>a.LogisticId!= logisticId&&a.LogisticStatus!=LogisticStatus.Compeleted)))
                {
                    poModel.POStatus = POStatus.Delivered;
                    newStatusLogs.IsDone = true;
                    //await _scmLogAndNotificationService.SetDonedNotificationAsync(userId, poModel.BaseContractCode, poModel.POId.ToString(), NotifEvent.AddPO);
                }

                await _poStatusLogRepository.AddAsync(newStatusLogs);
            }
            else
            {
                if (poModel.POStatus >= POStatus.packing && !await _packRepository.AnyAsync(a => !a.IsDeleted && a.POId == poModel.POId && a.Logistics.Any(a => a.LogisticId != logisticId && a.LogisticStatus != LogisticStatus.Compeleted)))
                {
                    poStatusLogs.IsDone = true;
                    poModel.POStatus = POStatus.Delivered;
                    //await _scmLogAndNotificationService.SetDonedNotificationAsync(userId, poModel.BaseContractCode, poModel.POId.ToString(), NotifEvent.AddPO);
                }
            }

            return true;
        }
    }
}
