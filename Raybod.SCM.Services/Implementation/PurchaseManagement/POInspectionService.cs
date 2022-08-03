using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.PO.POInspection;
using Raybod.SCM.DataTransferObject.User;
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
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class POInspectionService : IPOInspectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly DbSet<PAttachment> _pAttachmentRepository;
        private readonly DbSet<PO> _poRepository;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<POInspection> _inspectionRepository;
        private readonly CompanyAppSettingsDto _appSettings;
        private readonly Raybod.SCM.Services.Utilitys.FileHelper _fileHelper;

        public POInspectionService(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironmentRoot,
           ITeamWorkAuthenticationService authenticationService,
            IOptions<CompanyAppSettingsDto> appSettings,
            ISCMLogAndNotificationService scmLogAndNotificationService
            )
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _appSettings = appSettings.Value;
            _inspectionRepository = _unitOfWork.Set<POInspection>();
            _pAttachmentRepository = _unitOfWork.Set<PAttachment>();
            _userRepository = _unitOfWork.Set<User>();
            _poRepository = _unitOfWork.Set<PO>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
        }

        public async Task<ServiceResult<List<POInspectionDto>>> GetPoInspectionAsync (AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<POInspectionDto>>(null, MessageId.AccessDenied);

               

                var dbQuery = _inspectionRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.POId == poId &&
                    a.PO.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<POInspectionDto>>(null, MessageId.AccessDenied);

                var totalCount = dbQuery.Count();
                //dbComQuery = dbComQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new POInspectionDto
                {
                    POInspectionId = c.POInspectionId,
                    Description = c.Description,
                    Result = c.Result,
                    DueDate=c.DueDate.ToUnixTimestamp(),
                    ResultNote=c.ResultNote,
                    Attachments = c.Attachments.Where(a => !a.IsDeleted)
                     .Select(a => new POInspectionAttachmentDto
                     {
                         Id = a.Id,
                         FileName = a.FileName,
                         FileSize = a.FileSize,
                         FileSrc = a.FileSrc,
                         FileType = a.FileType,
                         POInspectionId = a.POInspectionId.Value
                     }).ToList(),

                    Inspector = c.Inspector != null ? new UserAuditLogDto
                    {
                        AdderUserId = c.InspectorId,
                        AdderUserName = c.Inspector.FullName,
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = !String.IsNullOrEmpty(c.Inspector.Image)?_appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.Inspector.Image:""
                    } : null,
                    UserAudit = c.AdderUser != null ? new UserAuditLogDto
                    {
                        AdderUserId = c.AdderUserId,
                        AdderUserName = c.AdderUser.FullName,
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = !String.IsNullOrEmpty(c.AdderUser.Image)?_appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.AdderUser.Image:""
                    } : null,
                    
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<POInspectionDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<UserMentionDto>>> GetInspectionUserListAsync(AuthenticateDto authenticate, long POId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateSuccess(new List<UserMentionDto>());

                var dbQuery = _poRepository
                    .Where(a => !a.IsDeleted && a.POId == POId && a.BaseContractCode == authenticate.ContractCode);

                var ProductGroupId = await dbQuery
                    .Select(c => c.ProductGroupId)
                    .FirstOrDefaultAsync();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                    return ServiceResultFactory.CreateSuccess(new List<UserMentionDto>());

                var roles = new List<string> { SCMRole.POInspectionMng };
                var list = await _authenticationService.GetAllUserHasAccessPOAsync(authenticate.ContractCode, roles, ProductGroupId);

                foreach (var item in list)
                {
                    item.Image = item.Image != null ?
                        _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + item.Image : "";
                }

                return ServiceResultFactory.CreateSuccess(list);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<UserMentionDto>>(null, exception);
            }
        }
        public async Task<ServiceResult<POInspectionDto>> AddPoInspectionAsync (AuthenticateDto authenticate, long poId, AddPOInspectionDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.AccessDenied);

                var dbQuery = _poRepository.Include(a=>a.Supplier)
                    .Where(a => !a.IsDeleted &&
                    a.POId == poId &&
                    !a.IsDeleted &&
                    a.BaseContractCode == authenticate.ContractCode);
                
                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.AccessDenied);
                var POModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (POModel == null)
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.EntityDoesNotExist);

                if (POModel.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.CantDoneBecausePOCanceled);

                var ownerUserModel = await _userRepository.FirstOrDefaultAsync(a => a.Id == model.InspectorId);
                if (ownerUserModel == null)
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.UserNotExist);

                var dActivityRole = new List<string> { SCMRole.POInspectionMng };
                var accessUserIds = await _authenticationService.GetAllUserHasAccessPOAsync(authenticate.ContractCode, dActivityRole, POModel.ProductGroupId);

                if (accessUserIds == null || !accessUserIds.Any() || !accessUserIds.Any(v => v.Id == model.InspectorId))
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.DataInconsistency);
                
                if (string.IsNullOrEmpty(model.Description))
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.InputDataValidationError);

               

                var PoInspection = new POInspection
                {
                    Description=model.Description,
                    InspectorId=model.InspectorId,
                    DueDate=model.DueDate.UnixTimestampToDateTime(),
                    Result=InspectionResult.NotStarted,
                    POId=poId
                };
                //if (model.Attachments != null && model.Attachments.Any())
                //{
                //    var insertAttachmentResult = await AddInspectionAttachmentAsync(PoInspection, model.Attachments);
                //    if(!insertAttachmentResult.Succeeded)
                //        return ServiceResultFactory.CreateError<List<InspectionDto>>(null, MessageId.UploudFailed);
                //}
                await _inspectionRepository.AddAsync(PoInspection);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await SendNotificationAndTaskOnAddActivityAsync(authenticate, POModel, PoInspection);
                    var result = await GetPoInspectionByIdAsync(authenticate, poId, PoInspection.POInspectionId);
                    if(result.Succeeded)
                        return ServiceResultFactory.CreateSuccess(result.Result);
                    else
                        return ServiceResultFactory.CreateSuccess(new POInspectionDto());
                }
                return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<POInspectionDto>(null, exception);
            }
        }
        public async Task<ServiceResult<POInspectionDto>> AddInspectionResultAsync(AuthenticateDto authenticate, long poId,long poInspectionId, AddPOInspectionResultDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.AccessDenied);

                var dbQuery = _inspectionRepository.Include(a=>a.PO).ThenInclude(a=>a.Supplier).Where(a =>
                    !a.PO.IsDeleted &&
                    a.POInspectionId == poInspectionId &&
                    a.POId == poId &&
                    !a.IsDeleted &&
                    a.PO.BaseContractCode == authenticate.ContractCode);


                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.EntityDoesNotExist);

                if (dbQuery.Any(a=>a.PO.POStatus==POStatus.Canceled))
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.EntityDoesNotExist);

                if (!dbQuery.Any(a => a.InspectorId == authenticate.UserId))
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.AccessDenied);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.AccessDenied);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.AccessDenied);
                var inspection = await dbQuery.FirstOrDefaultAsync();
                if (inspection==null)
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.EntityDoesNotExist);

                if (inspection.Result != InspectionResult.NotStarted)
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.InspecitonResultCantBeModified);

                if (model.Result == InspectionResult.NotStarted)
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.InspecitonResultCantBeModified);

                inspection.Result = model.Result;
                inspection.ResultNote = model.ResultNote;
                if (model.Attachments != null && model.Attachments.Any())
                {
                    var insertAttachmentResult = await AddInspectionAttachmentAsync(inspection, model.Attachments);
                    if (!insertAttachmentResult.Succeeded)
                        return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.UploudFailed);
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await LogAndTaskconfigOnSetPOStatusAsync(authenticate, inspection);

                    var result = await GetPoInspectionByIdAsync(authenticate, poId,inspection.POInspectionId);
                    if (result.Succeeded)
                        return ServiceResultFactory.CreateSuccess(result.Result);
                    else
                        return ServiceResultFactory.CreateSuccess(new POInspectionDto());
                    
                }
                return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<POInspectionDto>(null, exception);
            }
        }

        private async Task<ServiceResult<POInspection>> AddInspectionAttachmentAsync(POInspection inspectionModel, List<AddAttachmentDto> attachment)
        {
            inspectionModel.Attachments = new List<PAttachment>();

            foreach (var item in attachment)
            {
                var UploadedFile =
                    await _fileHelper.SaveDocumentFromTemp(item.FileSrc, ServiceSetting.UploadFilePath.POInspection);
                if (UploadedFile == null)
                    return ServiceResultFactory.CreateError<POInspection>(null, MessageId.UploudFailed);

                _fileHelper.DeleteDocumentFromTemp(item.FileSrc);
                inspectionModel.Attachments.Add(new PAttachment
                {
                    FileType = UploadedFile.FileType,
                    FileSize = UploadedFile.FileSize,
                    FileName = item.FileName,
                    FileSrc = item.FileSrc
                });
            }
            return ServiceResultFactory.CreateSuccess(inspectionModel);
        }

        public async Task<ServiceResult<bool>> DeletePoInspectionAsync(AuthenticateDto authenticate, long poId,long incpectionId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _inspectionRepository
                    .Where(a => !a.IsDeleted &&
                    a.POId == poId &&a.POInspectionId == incpectionId&&
                    !a.IsDeleted &&
                    a.PO.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var inspectionModel = await dbQuery
                    .FirstOrDefaultAsync();

                if (inspectionModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                inspectionModel.IsDeleted = true;

               
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {

                        return ServiceResultFactory.CreateSuccess(true);
                }
                return ServiceResultFactory.CreateError(false, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception) ;
            }
        }

        public async Task<DownloadFileDto> DownloadPOInspectionAttachmentAsync(AuthenticateDto authenticate, long poId, long inspectionId, string fileSrc)
        {
            
                try
                {
                    var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                    if (!permission.HasPermission)
                        return null;

                    var entity = await _inspectionRepository
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
                    var fileName = await _pAttachmentRepository.Where(a => !a.IsDeleted&&  a.POInspectionId==inspectionId&& a.FileSrc == fileSrc).FirstOrDefaultAsync();
                    if (fileName == null)
                        return null;
                    var streamResult = await _fileHelper.DownloadAttachmentDocument(fileSrc, ServiceSetting.FileSection.PoIncpection, fileName.FileName);
                    if (streamResult == null)
                        return null;

                    return streamResult;
                }
                catch (Exception exception)
                {
                    return null;
                }
            
        }

        private async Task<List<int>> GetPOInspectionUserIdsAsync(long poId, int poCreaterUserId)
        {
            var userIds = await _inspectionRepository
                      .Where(a => !a.IsDeleted &&
                      a.POId == poId)
                      .Select(c => c.InspectorId)
                      .ToListAsync();

            userIds.Add(poCreaterUserId);

            return userIds.Distinct().ToList();
        }
        private async Task SendNotificationAndTaskOnAddActivityAsync(AuthenticateDto authenticate, PO poModel, POInspection poInspectionModel)
        {
            var activityUserIds = await GetPOInspectionUserIdsAsync(poModel.POId, poModel.AdderUserId.Value);

            var logModel = new AddAuditLogDto
            {
                ContractCode = authenticate.ContractCode,
                Description = poModel.Supplier.Name,
                KeyValue = poInspectionModel.POInspectionId.ToString(),
                RootKeyValue = poModel.POId.ToString(),
                Message = poInspectionModel.Description,
                FormCode = poModel.POCode,
                Temp =  "",
                NotifEvent = NotifEvent.AddPOInspection,
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName,
                ProductGroupId = poModel.ProductGroupId,
                ReceiverLogUserIds = activityUserIds,
            };

            var taskModel = new AddTaskNotificationDto
            {
                ContractCode = authenticate.ContractCode,
                Description = poModel.Supplier.Name,
                Message = poInspectionModel.Description,
                Temp =  "",
                FormCode = poModel.POCode,
                Quantity = poInspectionModel.DueDate.ToUnixTimestamp().ToString(),
                KeyValue = poInspectionModel.POInspectionId.ToString(),
                NotifEvent = NotifEvent.AddPOInspection,
                RootKeyValue = poModel.POId.ToString(),
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName,
                Users = new List<int> { poInspectionModel.InspectorId }
            };

            var res1 = await _scmLogAndNotificationService.AddScmAuditLogAndTaskAsync(logModel, taskModel);
        }
        private async Task LogAndTaskconfigOnSetPOStatusAsync(AuthenticateDto authenticate, POInspection poInspectionModel)
        {
            await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, poInspectionModel.POInspectionId.ToString(), NotifEvent.AddPOInspection);

            List<int> activityUserIds = await GetPOInspectionUserIdsAsync(poInspectionModel.POId, poInspectionModel.PO.AdderUserId.Value);

            
                await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                {
                    ContractCode = authenticate.ContractCode,
                    Description = poInspectionModel.PO.Supplier.Name,
                    Message = poInspectionModel.Description,
                    FormCode = poInspectionModel.PO.POCode,
                    KeyValue = poInspectionModel.POInspectionId.ToString(),
                    NotifEvent =(poInspectionModel.Result==InspectionResult.OK)? NotifEvent.POInspectionPass:NotifEvent.POInspectionFailed,
                    RootKeyValue = poInspectionModel.POId.ToString(),
                    PerformerUserId = authenticate.UserId,
                    PerformerUserFullName = authenticate.UserFullName,
                    ProductGroupId = poInspectionModel.PO.ProductGroupId,
                    ReceiverLogUserIds = activityUserIds
                }, null);
        }

        private async Task<ServiceResult<POInspectionDto>> GetPoInspectionByIdAsync(AuthenticateDto authenticate, long poId,long poInspectionId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.AccessDenied);



                var dbQuery = _inspectionRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.POId == poId &&a.POInspectionId==poInspectionId&&
                    a.PO.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<POInspectionDto>(null, MessageId.AccessDenied);

                var totalCount = dbQuery.Count();
                //dbComQuery = dbComQuery.ApplayPageing(query);

                var result = await dbQuery.Select(c => new POInspectionDto
                {
                    POInspectionId = c.POInspectionId,
                    Description = c.Description,
                    Result = c.Result,
                    DueDate = c.DueDate.ToUnixTimestamp(),
                    ResultNote = c.ResultNote,
                    Attachments = c.Attachments.Where(a => !a.IsDeleted)
                     .Select(a => new POInspectionAttachmentDto
                     {
                         Id = a.Id,
                         FileName = a.FileName,
                         FileSize = a.FileSize,
                         FileSrc = a.FileSrc,
                         FileType = a.FileType,
                         POInspectionId = a.POInspectionId.Value
                     }).ToList(),

                    Inspector = c.Inspector != null ? new UserAuditLogDto
                    {
                        AdderUserId = c.InspectorId,
                        AdderUserName = c.Inspector.FullName,
                        CreateDate = c.CreatedDate.ToUnixTimestamp(),
                        AdderUserImage = !String.IsNullOrEmpty(c.Inspector.Image) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.Inspector.Image : ""
                    } : null,
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
                return ServiceResultFactory.CreateException<POInspectionDto>(null, exception);
            }
        }
    }
}
