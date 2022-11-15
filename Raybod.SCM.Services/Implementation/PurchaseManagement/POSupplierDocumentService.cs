using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.PO.POSupplierDocuemnt;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
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
    public class POSupplierDocumentService : IPOSupplierDocumentService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly DbSet<PAttachment> _pAttachmentRepository;
        private readonly DbSet<POSubject> _poSubjectRepository;
        private readonly DbSet<PO> _poRepository;
        private readonly DbSet<PoSupplierDocument> _poSupplierDocumentRepository;
        private readonly CompanyConfig _appSettings;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;

        public POSupplierDocumentService(IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
           ITeamWorkAuthenticationService authenticationService,
            IOptions<CompanyAppSettingsDto> appSettings,
            IHttpContextAccessor httpContextAccessor,
            ISCMLogAndNotificationService scmLogAndNotificationService
            )
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _poSupplierDocumentRepository = _unitOfWork.Set<PoSupplierDocument>();
            _pAttachmentRepository = _unitOfWork.Set<PAttachment>();
            _poSubjectRepository = _unitOfWork.Set<POSubject>();
            _poRepository = _unitOfWork.Set<PO>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        public async Task<ServiceResult<POSupplierDocumentDto>> AddPOSupplierDocumentAsync(AuthenticateDto authenticate, long poId, AddPOSupplierDocumentDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.AccessDenied);

                var dbQuery = _poRepository.Include(a=>a.Supplier)
                    .Where(a => !a.IsDeleted &&
                    a.POId == poId &&
                    !a.IsDeleted &&
                    a.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.AccessDenied);

                var POModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (POModel == null)
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.EntityDoesNotExist);

                if (POModel.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.CantDoneBecausePOCanceled);



                if (string.IsNullOrEmpty(model.DocumentTitle))
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.InputDataValidationError);

                if (string.IsNullOrEmpty(model.DocumentCode))
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.InputDataValidationError);



                var poSupplierDocument = new PoSupplierDocument
                {
                    DocumentTitle = model.DocumentTitle,
                    DocumentCode=model.DocumentCode,
                    POId = poId,
                    ProductId=model.ProdcutId
                };
                if (model.Attachments != null && model.Attachments.Any())
                {
                    var insertAttachmentResult = await AddPOSupplierDocumentAttachmentAsync(poSupplierDocument, model.Attachments);
                    if (!insertAttachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.UploudFailed);
                }
                else
                  return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.CannotSaveWithoutAttachment);
                
                await _poSupplierDocumentRepository.AddAsync(poSupplierDocument);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    

                    var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = POModel.Supplier.Name,
                        Message = poSupplierDocument.DocumentTitle,
                        FormCode = POModel.POCode,
                        KeyValue = poSupplierDocument.POSupplierDocumentId.ToString(),
                        Quantity = "",
                        NotifEvent = NotifEvent.AddPOSupplierDocument,
                        RootKeyValue = poId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        ProductGroupId = POModel.ProductGroupId,
                       
                    }, null);
                    var result = await GetPOSupplierDocumentByIdAsync(authenticate, poId, poSupplierDocument.POSupplierDocumentId);
                    if (result.Succeeded)
                        return ServiceResultFactory.CreateSuccess(result.Result);
                    else
                        return ServiceResultFactory.CreateSuccess(new POSupplierDocumentDto());
                }
                return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<POSupplierDocumentDto>(null, exception);
            }
        }
        public async Task<ServiceResult<POSupplierDocumentDto>> EditPOSupplierDocumentAsync(AuthenticateDto authenticate, long poId,long poSupplierDocumentId, EditPOSupplierDocumentDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.AccessDenied);

                var dbQuery = _poSupplierDocumentRepository.Include(a=>a.Attachments).Include(a=>a.PO).ThenInclude(a => a.Supplier)
                    .Where(a => !a.IsDeleted &&
                    a.POId == poId &&a.POSupplierDocumentId==poSupplierDocumentId&&
                    !a.IsDeleted &&
                    a.PO.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.AccessDenied);

                var poSupplierDocument = await dbQuery
                    .FirstOrDefaultAsync();


                if (poSupplierDocument == null)
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.EntityDoesNotExist);

                if (poSupplierDocument.PO.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.CantDoneBecausePOCanceled);

                if (string.IsNullOrEmpty(model.DocumentTitle))
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.InputDataValidationError);

                if (string.IsNullOrEmpty(model.DocumentCode))
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.InputDataValidationError);

                poSupplierDocument.DocumentTitle = model.DocumentTitle;
                poSupplierDocument.ProductId = model.ProdcutId;
                poSupplierDocument.DocumentCode = model.DocumentCode;

                if (model.Attachments != null && model.Attachments.Any())
                {
                    var insertAttachmentResult = await EditPOSupplierDocumentAttachmentAsync(poSupplierDocument, model.Attachments);
                    if (!insertAttachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.UploudFailed);
                }
                else
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.CannotSaveWithoutAttachment);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {


                    var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = poSupplierDocument.PO.Supplier.Name,
                        Message = poSupplierDocument.DocumentTitle,
                        FormCode = poSupplierDocument.PO.POCode,
                        KeyValue = poSupplierDocument.POSupplierDocumentId.ToString(),
                        Quantity = "",
                        NotifEvent = NotifEvent.EditPOSupplierDocument,
                        RootKeyValue = poId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        ProductGroupId = poSupplierDocument.PO.ProductGroupId,

                    }, null);
                    var result = await GetPOSupplierDocumentByIdAsync(authenticate, poId, poSupplierDocument.POSupplierDocumentId);
                    if (result.Succeeded)
                        return ServiceResultFactory.CreateSuccess(result.Result);
                    else
                        return ServiceResultFactory.CreateSuccess(new POSupplierDocumentDto());
                }
                return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<POSupplierDocumentDto>(null, exception);
            }
        }
        public async Task<ServiceResult<bool>> DeletePOSupplierDocumentAsync(AuthenticateDto authenticate, long poId,long poSupplierDocumentId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _poSupplierDocumentRepository
                    .Where(a => !a.IsDeleted &&
                    a.POId == poId &&a.POSupplierDocumentId== poSupplierDocumentId &&
                    !a.IsDeleted &&
                    a.PO.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var manufactureModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (manufactureModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                manufactureModel.IsDeleted = true;


                if (await _unitOfWork.SaveChangesAsync() > 0)
                {

                    return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<DownloadFileDto> DownloadPOSupplierDocumentAttachmentAsync(AuthenticateDto authenticate, long poId, long poSupplierDocumentId, string fileSrc)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var entity = await _poSupplierDocumentRepository
                   .Where(a => !a.IsDeleted &&
                   a.POId == poId &&
                   a.PO.BaseContractCode == authenticate.ContractCode &&
                   a.Attachments.Any(c => !c.IsDeleted && c.FileSrc == fileSrc))
                   .Select(c => new
                   {
                       ContractCode = c.PO.BaseContractCode,
                       ProductGroupId = c.PO.ProductGroupId,
                   }).FirstOrDefaultAsync();

                if (entity == null)
                    return null;

                if (permission.ProductGroupIds.Any() && !permission.ProductGroupIds.Contains(entity.ProductGroupId))
                    return null;
                var fileName = await _pAttachmentRepository.Where(a => !a.IsDeleted && a.POSupplierDocumentId == poSupplierDocumentId && a.FileSrc == fileSrc).FirstOrDefaultAsync();
                if (fileName == null)
                    return null;
                var streamResult = await _fileHelper.DownloadAttachmentDocument(fileSrc, ServiceSetting.FileSection.ManufactureDocument, fileName.FileName);
                if (streamResult == null)
                    return null;

                return streamResult;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<ServiceResult<List<POSupplierDocumentDto>>> GetPOSupplierDocumentAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<POSupplierDocumentDto>>(null, MessageId.AccessDenied);



                var dbQuery = _poSupplierDocumentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.POId == poId &&
                    a.PO.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<POSupplierDocumentDto>>(null, MessageId.AccessDenied);

                var totalCount = dbQuery.Count();
                //dbComQuery = dbComQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new POSupplierDocumentDto
                {
                    POSupplierDocumentId = c.POSupplierDocumentId,
                    ProductId=c.Product.Id,
                    ProductCode=c.Product.ProductCode,
                    ProductGroupName=c.Product.ProductGroup.Title,
                    ProductName=c.Product.Description,
                    ProductUnit=c.Product.Unit,
                    TechnicalNumber=c.Product.TechnicalNumber,
                    DocumentTitle = c.DocumentTitle,
                    DocumentCode=c.DocumentCode,
                    Attachments = c.Attachments.Where(a => !a.IsDeleted)
                     .Select(a => new POSupplierDocumentAttachmentDto
                     {
                         Id = a.Id,
                         FileName = a.FileName,
                         FileSize = a.FileSize,
                         FileSrc = a.FileSrc,
                         FileType = a.FileType,
                         POSupplierDocumentId = a.POSupplierDocumentId.Value
                     }).ToList(),

                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = c.AdderUserId,
                        AdderUserName = c.AdderUser.FullName,
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage =!String.IsNullOrEmpty(c.AdderUser.Image) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image:""
                    } : null,

                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<POSupplierDocumentDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<POSupplierDocumentProductListDto>>> GetPOSupplierDocumentProductListAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<POSupplierDocumentProductListDto>>(null, MessageId.AccessDenied);



                var dbQuery = _poSubjectRepository
                    .AsNoTracking()
                    .Where(a =>  a.POId == poId &&
                    a.PO.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<POSupplierDocumentProductListDto>>(null, MessageId.AccessDenied);

                var totalCount = dbQuery.Count();
                //dbComQuery = dbComQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new POSupplierDocumentProductListDto
                {
                    ProductId = c.ProductId,
                    ProductCode =c.Product.ProductCode,
                    ProductGroupName=c.Product.ProductGroup.Title,
                    ProductName=c.Product.Description,
                    ProductUnit=c.Product.Unit,
                    TechnicalNumber=c.Product.TechnicalNumber
                }).ToListAsync();
                List<POSupplierDocumentProductListDto> products = new List<POSupplierDocumentProductListDto>();
                foreach(var item in result)
                {
                    if (!products.Any(a => a.ProductId == item.ProductId))
                    {
                        products.Add(item);
                    }
                }
                return ServiceResultFactory.CreateSuccess(products).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<POSupplierDocumentProductListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<POSupplierDocumentDto>>> GetPOProductAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<POSupplierDocumentDto>>(null, MessageId.AccessDenied);



                var dbQuery = _poSupplierDocumentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.POId == poId &&
                    a.PO.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<POSupplierDocumentDto>>(null, MessageId.AccessDenied);

                var totalCount = dbQuery.Count();
                //dbComQuery = dbComQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new POSupplierDocumentDto
                {
                    POSupplierDocumentId = c.POSupplierDocumentId,
                    ProductCode = c.Product.ProductCode,
                    ProductGroupName = c.Product.ProductGroup.Title,
                    ProductName = c.Product.Description,
                    ProductUnit = c.Product.Unit,
                    TechnicalNumber = c.Product.TechnicalNumber,
                    DocumentTitle = c.DocumentTitle,
                    DocumentCode = c.DocumentCode,
                    Attachments = c.Attachments.Where(a => !a.IsDeleted)
                     .Select(a => new POSupplierDocumentAttachmentDto
                     {
                         Id = a.Id,
                         FileName = a.FileName,
                         FileSize = a.FileSize,
                         FileSrc = a.FileSrc,
                         FileType = a.FileType,
                         POSupplierDocumentId = a.POSupplierDocumentId.Value
                     }).ToList(),

                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = c.AdderUserId,
                        AdderUserName = c.AdderUser.FullName,
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = !String.IsNullOrEmpty(c.AdderUser.Image) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : ""
                    } : null,

                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<POSupplierDocumentDto>>(null, exception);
            }
        }
       
        private async Task<ServiceResult<PoSupplierDocument>> AddPOSupplierDocumentAttachmentAsync(PoSupplierDocument poSupplierDocumentModel, List<AddAttachmentDto> attachment)
        {
            poSupplierDocumentModel.Attachments = new List<PAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.POManufactureDocument);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<PoSupplierDocument>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                poSupplierDocumentModel.Attachments.Add(new PAttachment
                {
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc
                });
            }
            return ServiceResultFactory.CreateSuccess(poSupplierDocumentModel);
        }

        private async Task<ServiceResult<PoSupplierDocument>> EditPOSupplierDocumentAttachmentAsync(PoSupplierDocument poSupplierDocumentModel, List<POSupplierDocumentEditAttachmentDto> attachment)
        {

            var attachmentIds = attachment.Where(a => a.POSupplierDocumentId != null).Select(a => a.Id);
            foreach (var attach in poSupplierDocumentModel.Attachments)
            {
                if (!attachmentIds.Contains(attach.Id))
                    attach.IsDeleted = true;
            }
            foreach (var item in attachment)
            {
                if (item.POSupplierDocumentId == null)
                {
                    var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.POManufactureDocument);
                    if (UploadedFile == null)
                        return ServiceResultFactory.CreateError<PoSupplierDocument>(null, MessageId.UploudFailed);

                    _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                    poSupplierDocumentModel.Attachments.Add(new PAttachment
                    {
                        FileType = UploadedFile.FileType,
                        FileSize = UploadedFile.FileSize,
                        FileName = item.FileName,
                        FileSrc = item.FileSrc
                    });
                }
                
            }
           
                
            
            return ServiceResultFactory.CreateSuccess(poSupplierDocumentModel);
        }
        private async Task<ServiceResult<POSupplierDocumentDto>> GetPOSupplierDocumentByIdAsync(AuthenticateDto authenticate, long poId,long poSupplierDocumentId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.AccessDenied);



                var dbQuery = _poSupplierDocumentRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.POId == poId &&a.POSupplierDocumentId== poSupplierDocumentId &&
                    a.PO.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<POSupplierDocumentDto>(null, MessageId.AccessDenied);

                var totalCount = dbQuery.Count();
                //dbComQuery = dbComQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new POSupplierDocumentDto
                {
                    POSupplierDocumentId = c.POSupplierDocumentId,
                    ProductId = c.Product.Id,
                    ProductCode = c.Product.ProductCode,
                    ProductGroupName = c.Product.ProductGroup.Title,
                    ProductName = c.Product.Description,
                    ProductUnit = c.Product.Unit,
                    TechnicalNumber = c.Product.TechnicalNumber,
                    DocumentTitle = c.DocumentTitle,
                    DocumentCode = c.DocumentCode,
                    Attachments = c.Attachments.Where(a => !a.IsDeleted)
                     .Select(a => new POSupplierDocumentAttachmentDto
                     {
                         Id = a.Id,
                         FileName = a.FileName,
                         FileSize = a.FileSize,
                         FileSrc = a.FileSrc,
                         FileType = a.FileType,
                         POSupplierDocumentId = a.POSupplierDocumentId.Value
                     }).ToList(),

                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = c.AdderUserId,
                        AdderUserName = c.AdderUser.FullName,
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = !String.IsNullOrEmpty(c.AdderUser.Image) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image : ""
                    } : null,

                }).FirstOrDefaultAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<POSupplierDocumentDto>(null, exception);
            }
        }
    }
}

