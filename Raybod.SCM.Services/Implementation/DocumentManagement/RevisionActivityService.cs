using Exon.TheWeb.Service.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Document;
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
using System.Threading.Tasks;



namespace Raybod.SCM.Services.Implementation
{
    public class RevisionActivityService : IRevisionActivityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<DocumentRevision> _documentRevisionRepository;
        private readonly DbSet<RevisionActivity> _revisionActivityRepository;
        private readonly DbSet<RevisionActivityTimesheet> _activityTimesheetRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyAppSettingsDto _appSettings;

        public RevisionActivityService(IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            IOptions<CompanyAppSettingsDto> appSettings,
            ITeamWorkAuthenticationService authenticationServices,
            ISCMLogAndNotificationService scmLogAndNotificationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationServices = authenticationServices;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _appSettings = appSettings.Value;
            _userRepository = _unitOfWork.Set<User>();
            _documentRevisionRepository = _unitOfWork.Set<DocumentRevision>();
            _revisionActivityRepository = _unitOfWork.Set<RevisionActivity>();
            _activityTimesheetRepository = _unitOfWork.Set<RevisionActivityTimesheet>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
        }

        #region revision activity
        public async Task<ServiceResult<BaseRevisionAvtivityDto>> AddRevisionActivityAsync(AuthenticateDto authenticate, long documentId, long revisionId,
            AddRevisionActivityDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.AccessDenied);

                var dbQuery = _documentRevisionRepository
                    .Where(a => !a.IsDeleted &&
                    a.DocumentRevisionId == revisionId &&
                    a.Document.IsActive &&
                    a.Document.ContractCode == authenticate.ContractCode &&
                    a.DocumentId == documentId);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.AccessDenied);

                var documentRevisionModel = await dbQuery
                    .Include(a => a.Document)
                    .FirstOrDefaultAsync();

                if (documentRevisionModel == null)
                    return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.EntityDoesNotExist);

                var ownerUserModel = await _userRepository.FirstOrDefaultAsync(a => a.Id == model.ActivityOwnerId);
                if (ownerUserModel == null)
                    return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.UserNotExist);

                var dActivityRole = new List<string> { SCMRole.RevisionActivityMng, SCMRole.RevisionMng };
                var accessUserIds = await _authenticationServices
                    .GetAllUserHasAccessDocumentAsync(authenticate.ContractCode, dActivityRole, documentRevisionModel.Document.DocumentGroupId);

                if (string.IsNullOrEmpty(model.Description) || model.ActivityOwnerId <= 0)
                    return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.InputDataValidationError);

                if (accessUserIds == null || !accessUserIds.Any() || !accessUserIds.Any(v => v.Id == model.ActivityOwnerId))
                    return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.ResponsibleHaveNotAcess);

                var revisionActivityModel = new RevisionActivity
                {
                    DateEnd = model.DateEnd.UnixTimestampToDateTime(),
                    Description = model.Description,
                    ActivityOwnerId = model.ActivityOwnerId,
                    RevisionId = revisionId,
                    RevisionActivityStatus = RevisionActivityStatus.Todo,
                };

                _revisionActivityRepository.Add(revisionActivityModel);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new BaseRevisionAvtivityDto
                    {
                        ActivityOwnerId = revisionActivityModel.ActivityOwnerId,
                        DateEnd = revisionActivityModel.DateEnd.ToUnixTimestamp(),
                        Description = revisionActivityModel.Description,
                        Duration = $"{Math.Floor(revisionActivityModel.Duration)}:{TimeSpan.FromHours(revisionActivityModel.Duration).Minutes}",
                        RevisionActivityId = revisionActivityModel.RevisionActivityId,
                        RevisionActivityStatus = revisionActivityModel.RevisionActivityStatus,
                        ActivityOwner = new UserAuditLogDto
                        {
                            AdderUserName = ownerUserModel.FullName,
                            AdderUserImage = ownerUserModel.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + ownerUserModel.Image : ""
                        }
                    };

                    await SendNotificationAndTaskOnAddActivityAsync(authenticate, documentRevisionModel, revisionActivityModel);

                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseRevisionAvtivityDto>(null, exception);
            }
        }

        private async Task SendNotificationAndTaskOnAddActivityAsync(AuthenticateDto authenticate, DocumentRevision documentRevisionModel, RevisionActivity revisionActivityModel)
        {
            var activityUserIds = await GetRevisionActivityUserIdsAsync(documentRevisionModel.DocumentRevisionId, documentRevisionModel.AdderUserId.Value);

            var logModel = new AddAuditLogDto
            {
                ContractCode = authenticate.ContractCode,
                Description = documentRevisionModel.Document.DocTitle,
                Message = revisionActivityModel.Description,
                FormCode = documentRevisionModel.DocumentRevisionCode,
                KeyValue = revisionActivityModel.RevisionActivityId.ToString(),
                NotifEvent = NotifEvent.AddRevisionActivity,
                RootKeyValue = documentRevisionModel.DocumentRevisionId.ToString(),
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName,
                DocumentGroupId = documentRevisionModel.Document.DocumentGroupId,
                ReceiverLogUserIds = activityUserIds
            };

            var taskModel = new AddTaskNotificationDto
            {
                ContractCode = authenticate.ContractCode,
                Description = documentRevisionModel.Document.DocTitle,
                Message = revisionActivityModel.Description,
                FormCode = documentRevisionModel.DocumentRevisionCode,
                Quantity = revisionActivityModel.DateEnd.ToUnixTimestamp().ToString(),
                KeyValue = revisionActivityModel.RevisionActivityId.ToString(),
                NotifEvent = NotifEvent.AddRevisionActivity,
                RootKeyValue = documentRevisionModel.DocumentRevisionId.ToString(),
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName,
                Users = new List<int> { revisionActivityModel.ActivityOwnerId }
            };

            var res1 = await _scmLogAndNotificationService.AddScmAuditLogAndTaskAsync(logModel, taskModel);
        }

        public async Task<ServiceResult<bool>> SetRevisionActivityStatusAsync(AuthenticateDto authenticate, long documentId, long revisionId,
            long revisionActivityId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _revisionActivityRepository
                    .Where(a => a.RevisionActivityId == revisionActivityId &&
                    !a.DocumentRevision.IsDeleted &&
                     a.DocumentRevision.DocumentRevisionId == revisionId &&
                     a.DocumentRevision.DocumentId == documentId &&
                     a.DocumentRevision.Document.IsActive &&
                     a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (!dbQuery.Any(a => a.ActivityOwnerId == authenticate.UserId))
                    return ServiceResultFactory.CreateError(false, MessageId.OnlyResponsibleCanMakeChangeInActivity);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);


                var revisionActivityModel = await dbQuery
                    .Include(a => a.RevisionActivityTimesheets)
                    .Include(a => a.DocumentRevision)
                    .ThenInclude(a => a.Document)
                    .FirstOrDefaultAsync();

                if (revisionActivityModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (revisionActivityModel.RevisionActivityStatus == RevisionActivityStatus.Todo)
                    revisionActivityModel.RevisionActivityStatus = RevisionActivityStatus.Done;
                else
                    revisionActivityModel.RevisionActivityStatus = RevisionActivityStatus.Todo;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await LogAndTaskconfigOnSetRevisionStatusAsync(authenticate, revisionActivityModel);
                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private async Task LogAndTaskconfigOnSetRevisionStatusAsync(AuthenticateDto authenticate, RevisionActivity revisionActivityModel)
        {
            await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, revisionActivityModel.RevisionActivityId.ToString(), NotifEvent.AddRevisionActivity);

            List<int> activityUserIds = await GetRevisionActivityUserIdsAsync(revisionActivityModel.RevisionId, revisionActivityModel.DocumentRevision.AdderUserId.Value);

            if (revisionActivityModel.RevisionActivityStatus == RevisionActivityStatus.Done)
                await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                {
                    ContractCode = authenticate.ContractCode,
                    Description = revisionActivityModel.DocumentRevision.Document.DocTitle,
                    Message = revisionActivityModel.Description,
                    FormCode = revisionActivityModel.DocumentRevision.DocumentRevisionCode,
                    KeyValue = revisionActivityModel.RevisionActivityId.ToString(),
                    NotifEvent = NotifEvent.RevisionActivityDone,
                    RootKeyValue = revisionActivityModel.RevisionId.ToString(),
                    PerformerUserId = authenticate.UserId,
                    PerformerUserFullName = authenticate.UserFullName,
                    DocumentGroupId = revisionActivityModel.DocumentRevision.Document.DocumentGroupId,
                    ReceiverLogUserIds = activityUserIds
                }, null);
        }

        private async Task<List<int>> GetRevisionActivityUserIdsAsync(long revisionId, int revisionCreaterUserId)
        {
            var userIds = await _revisionActivityRepository
                      .Where(a => !a.IsDeleted &&
                      a.RevisionId == revisionId)
                      .Select(c => c.ActivityOwnerId)
                      .ToListAsync();

            userIds.Add(revisionCreaterUserId);

            return userIds.Distinct().ToList();
        }

        public async Task<ServiceResult<bool>> DeleteRevisionActivityAsync(AuthenticateDto authenticate, long documentId, long revisionId,
            long revisionActivityId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _revisionActivityRepository
                    .Where(a => a.RevisionActivityId == revisionActivityId &&
                     !a.DocumentRevision.IsDeleted &&
                     a.DocumentRevision.DocumentRevisionId == revisionId &&
                     a.DocumentRevision.Document.IsActive && a.DocumentRevision.Document.ContractCode == authenticate.ContractCode && a.DocumentRevision.DocumentId == documentId);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);


                var revisionActivityModel = await dbQuery
                    .Include(a => a.RevisionActivityTimesheets)
                     .Include(a => a.DocumentRevision)
                    .ThenInclude(a => a.Document)
                    .FirstOrDefaultAsync();

                if (revisionActivityModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                revisionActivityModel.IsDeleted = true;

                foreach (var item in revisionActivityModel.RevisionActivityTimesheets)
                {
                    item.IsDeleted = true;
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    List<int> activityUserIds = await GetRevisionActivityUserIdsAsync(revisionActivityModel.RevisionId, revisionActivityModel.DocumentRevision.AdderUserId.Value);

                    var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = revisionActivityModel.DocumentRevision.Document.DocTitle,
                        Message = revisionActivityModel.Description,
                        FormCode = revisionActivityModel.DocumentRevision.DocumentRevisionCode,
                        KeyValue = revisionActivityModel.RevisionActivityId.ToString(),
                        NotifEvent = NotifEvent.DeleteRevisionActivity,
                        RootKeyValue = revisionActivityModel.RevisionId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = revisionActivityModel.DocumentRevision.Document.DocumentGroupId,
                        ReceiverLogUserIds = activityUserIds
                    }, null);
                    await _scmLogAndNotificationService.RemoveNotificationAsync(authenticate.ContractCode, revisionActivityModel.RevisionActivityId.ToString(), NotifEvent.AddRevisionActivity);

                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.DeleteEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        #endregion

        public async Task<ServiceResult<BaseRevisionAvtivityDto>> EditRevisionActivityAsync(AuthenticateDto authenticate, long documentId,
            long revisionId, long revisionActivityId, AddRevisionActivityDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.AccessDenied);

                var dbQuery = _revisionActivityRepository
                  .Where(a => a.RevisionActivityId == revisionActivityId &&
                   !a.DocumentRevision.IsDeleted &&
                   a.DocumentRevision.DocumentRevisionId == revisionId &&
                   a.DocumentRevision.Document.IsActive &&
                   a.DocumentRevision.Document.ContractCode == authenticate.ContractCode &&
                   a.DocumentRevision.DocumentId == documentId);


                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.AccessDenied);

                var revisionActivityModel = await dbQuery
                    .Include(a => a.ActivityOwner)
                    .Include(a => a.DocumentRevision)
                    .ThenInclude(a => a.Document)
                    .FirstOrDefaultAsync();

                if (revisionActivityModel == null)
                    return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.EntityDoesNotExist);

                if (string.IsNullOrEmpty(model.Description) || model.ActivityOwnerId <= 0)
                    return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.InputDataValidationError);


                if (model.ActivityOwnerId != revisionActivityModel.ActivityOwnerId)
                {
                    var ownerUserModel = await _userRepository.FirstOrDefaultAsync(a => a.Id == model.ActivityOwnerId);
                    if (ownerUserModel == null)
                        return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.UserNotExist);

                    revisionActivityModel.ActivityOwnerId = model.ActivityOwnerId;
                    revisionActivityModel.ActivityOwner = ownerUserModel;
                }

                var dActivityRole = new List<string> { SCMRole.RevisionActivityMng, SCMRole.RevisionMng };
                var accessUserIds = await _authenticationServices
                    .GetAllUserHasAccessDocumentAsync(authenticate.ContractCode, dActivityRole, revisionActivityModel.DocumentRevision.Document.DocumentGroupId);


                if (accessUserIds == null || !accessUserIds.Any() || !accessUserIds.Any(v => v.Id == model.ActivityOwnerId))
                    return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.ResponsibleHaveNotAcess);

                revisionActivityModel.DateEnd = model.DateEnd.UnixTimestampToDateTime();
                revisionActivityModel.Description = model.Description;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    //await _scmLogAndNotificationService.RemoveNotificationAsync(authenticate.ContractCode, revisionActivityModel.RevisionActivityId.ToString(), NotifEvent.AddRevisionActivity);

                    List<int> activityUserIds = await GetRevisionActivityUserIdsAsync(revisionActivityModel.RevisionId, revisionActivityModel.DocumentRevision.AdderUserId.Value);

                    var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = revisionActivityModel.DocumentRevision.Document.DocTitle,
                        Message = revisionActivityModel.Description,
                        FormCode = revisionActivityModel.DocumentRevision.DocumentRevisionCode,
                        KeyValue = revisionActivityModel.RevisionActivityId.ToString(),
                        NotifEvent = NotifEvent.EditRevisionActivity,
                        RootKeyValue = revisionActivityModel.RevisionId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = revisionActivityModel.DocumentRevision.Document.DocumentGroupId,
                        ReceiverLogUserIds = activityUserIds
                    }, null);
                }

                var result = new BaseRevisionAvtivityDto
                {
                    ActivityOwnerId = revisionActivityModel.ActivityOwnerId,
                    DateEnd = revisionActivityModel.DateEnd.ToUnixTimestamp(),
                    Description = revisionActivityModel.Description,
                    Duration = $"{Math.Floor(revisionActivityModel.Duration)}:{TimeSpan.FromHours(revisionActivityModel.Duration).Minutes}",
                    RevisionActivityId = revisionActivityModel.RevisionActivityId,
                    RevisionActivityStatus = revisionActivityModel.RevisionActivityStatus,
                    ActivityOwner = new UserAuditLogDto
                    {
                        AdderUserName = revisionActivityModel.ActivityOwner.FullName,
                        AdderUserImage = revisionActivityModel.ActivityOwner.Image != null ?
                        _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + revisionActivityModel.ActivityOwner.Image : ""
                    }
                };
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BaseRevisionAvtivityDto>(null, exception);
            }
        }

        #region revision TimeSheet
        public async Task<ServiceResult<ResultAddActivityTimeSheetDto>> AddActivityTimeSheetAsync(AuthenticateDto authenticate, long documentId, long revisionId,
            long revisionActivityId, AddActivityTimeSheetDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.AccessDenied);

                var dbQuery = _revisionActivityRepository.Where(a =>
                    !a.DocumentRevision.IsDeleted &&
                    a.RevisionActivityId == revisionActivityId &&
                    a.DocumentRevision.DocumentRevisionId == revisionId &&
                    a.DocumentRevision.DocumentId == documentId &&
                    a.DocumentRevision.Document.IsActive &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);


                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.EntityDoesNotExist);

                if (!dbQuery.Any(a => a.ActivityOwnerId == authenticate.UserId))
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.OnlyResponsibleCanMakeChangeInActivity);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.AccessDenied);

                var revisionActivityModel = await dbQuery
                    .Include(a => a.RevisionActivityTimesheets)
                    .Include(a => a.DocumentRevision)
                    .ThenInclude(a => a.Document)
                    .FirstOrDefaultAsync();

                if (revisionActivityModel == null)
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.EntityDoesNotExist);

                TimeSpan ts = new TimeSpan();
                try
                {
                    ts = TimeSpan.Parse(model.Duration);
                }
                catch (FormatException)
                {
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.InputDataValidationError);
                    //Console.WriteLine("{0}: Bad Format", value);
                }
                catch (OverflowException)
                {
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.InputDataValidationError);
                    //Console.WriteLine("{0}: Overflow", value);
                }

                var activityTimeSheetModel = new RevisionActivityTimesheet
                {
                    DateIssue = model.DateIssue.UnixTimestampToDateTime(),
                    Description = model.Description,
                    RevisionActivityId = revisionActivityModel.RevisionActivityId,
                    Duration = ts
                };

                if (revisionActivityModel.RevisionActivityTimesheets != null && revisionActivityModel.RevisionActivityTimesheets.Any())
                    revisionActivityModel.RevisionActivityTimesheets.Add(activityTimeSheetModel);
                else
                {
                    revisionActivityModel.RevisionActivityTimesheets = new List<RevisionActivityTimesheet>();
                    revisionActivityModel.RevisionActivityTimesheets.Add(activityTimeSheetModel);
                }

                var sumDurationTimeSpan = new TimeSpan(revisionActivityModel.RevisionActivityTimesheets.Where(a => !a.IsDeleted).Sum(v => v.Duration.Ticks));
                revisionActivityModel.Duration = sumDurationTimeSpan.TotalHours;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new ResultAddActivityTimeSheetDto
                    {
                        TotalDuration = $"{Math.Floor(sumDurationTimeSpan.TotalHours)}:{sumDurationTimeSpan.Minutes}",
                        ActivityTimesheetId = activityTimeSheetModel.ActivityTimesheetId,
                        DateIssue = activityTimeSheetModel.DateIssue.ToUnixTimestamp(),
                        Description = activityTimeSheetModel.Description,
                        Duration = activityTimeSheetModel.Duration.ToString(@"hh\:mm"),
                        UserAudit = new UserAuditLogDto
                        {
                            AdderUserName = authenticate.UserFullName,
                            AdderUserImage = authenticate.UserImage != null ? authenticate.UserImage : ""
                        }
                    };

                    List<int> activityUserIds = await GetRevisionActivityUserIdsAsync(revisionActivityModel.RevisionId, revisionActivityModel.DocumentRevision.AdderUserId.Value);

                    var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = revisionActivityModel.DocumentRevision.Document.DocTitle,
                        Message = revisionActivityModel.Description,
                        FormCode = revisionActivityModel.DocumentRevision.DocumentRevisionCode,
                        KeyValue = revisionActivityModel.RevisionActivityId.ToString(),
                        Quantity = activityTimeSheetModel.Duration.ToString(@"hh\:mm"),
                        NotifEvent = NotifEvent.AddRevisionTimeSheet,
                        RootKeyValue = revisionActivityModel.RevisionId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = revisionActivityModel.DocumentRevision.Document.DocumentGroupId,
                        ReceiverLogUserIds = activityUserIds
                    }, null);

                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ResultAddActivityTimeSheetDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ListActivityTimeSheetDto>>> GetActivityTimeSheetAsync(AuthenticateDto authenticate, long documentId, long revisionId,
            long revisionActivityId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListActivityTimeSheetDto>>(null, MessageId.AccessDenied);

                var dbQuery = _revisionActivityRepository
                    .AsNoTracking()
                    .Where(a => !a.DocumentRevision.IsDeleted &&
                    a.RevisionActivityId == revisionActivityId &&
                    a.DocumentRevision.DocumentRevisionId == revisionId &&
                    a.DocumentRevision.DocumentId == documentId &&
                    a.DocumentRevision.Document.IsActive &&
                    a.DocumentRevision.Document.ContractCode == authenticate.ContractCode);


                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<ListActivityTimeSheetDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError<List<ListActivityTimeSheetDto>>(null, MessageId.AccessDenied);

                var result = await dbQuery.SelectMany(c => c.RevisionActivityTimesheets.Where(v => !v.IsDeleted))
                    .Select(v => new ListActivityTimeSheetDto
                    {
                        ActivityTimesheetId = v.ActivityTimesheetId,
                        DateIssue = v.DateIssue.ToUnixTimestamp(),
                        Description = v.Description,
                        Duration = v.Duration.ToString(@"hh\:mm"),
                        UserAudit = v.AdderUser != null ? new UserAuditLogDto
                        {
                            AdderUserName = v.AdderUser.FullName,
                            AdderUserImage = v.AdderUser.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + v.AdderUser.Image : ""
                        } : null
                    }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ListActivityTimeSheetDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<string>> DeleteActivityTimeSheetAsync(AuthenticateDto authenticate, long documentId, long revisionId,
            long revisionActivityId, long activityTimeSheetId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var dbQuery = _activityTimesheetRepository.Where(a =>
                    !a.IsDeleted &&
                    a.ActivityTimesheetId == activityTimeSheetId &&
                    !a.RevisionActivity.DocumentRevision.IsDeleted &&
                    a.RevisionActivity.RevisionActivityId == revisionActivityId &&
                    a.RevisionActivity.DocumentRevision.DocumentRevisionId == revisionId &&
                    a.RevisionActivity.DocumentRevision.DocumentId == documentId &&
                    a.RevisionActivity.DocumentRevision.Document.IsActive &&
                    a.RevisionActivity.DocumentRevision.Document.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                if (!dbQuery.Any(a => a.RevisionActivity.ActivityOwnerId == authenticate.UserId))
                    return ServiceResultFactory.CreateError("", MessageId.OnlyResponsibleCanMakeChangeInActivity);

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.RevisionActivity.DocumentRevision.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateError("", MessageId.AccessDenied);

                var activityTimeSheetModel = await dbQuery
                    .Include(a => a.RevisionActivity)
                    .ThenInclude(a => a.DocumentRevision)
                    .ThenInclude(a => a.Document)
                    .FirstOrDefaultAsync();

                if (activityTimeSheetModel == null)
                    return ServiceResultFactory.CreateError("", MessageId.EntityDoesNotExist);

                activityTimeSheetModel.IsDeleted = true;

                var totalHour = activityTimeSheetModel.Duration.TotalHours;
                activityTimeSheetModel.RevisionActivity.Duration -= totalHour;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    List<int> activityUserIds = await GetRevisionActivityUserIdsAsync(activityTimeSheetModel.RevisionActivity.RevisionId,
                        activityTimeSheetModel.RevisionActivity.DocumentRevision.AdderUserId.Value);

                    var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = activityTimeSheetModel.RevisionActivity.DocumentRevision.Document.DocTitle,
                        Message = activityTimeSheetModel.Description,
                        FormCode = activityTimeSheetModel.RevisionActivity.DocumentRevision.DocumentRevisionCode,
                        KeyValue = activityTimeSheetModel.RevisionActivityId.ToString(),
                        Quantity = activityTimeSheetModel.Duration.ToString(@"hh\:mm"),
                        NotifEvent = NotifEvent.DeleteRevisionTimeSheet,
                        RootKeyValue = activityTimeSheetModel.RevisionActivity.RevisionId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        DocumentGroupId = activityTimeSheetModel.RevisionActivity.DocumentRevision.Document.DocumentGroupId,
                        ReceiverLogUserIds = activityUserIds
                    }, null);

                    return ServiceResultFactory.CreateSuccess(activityTimeSheetModel.RevisionActivity.Duration.ToString());
                }

                return ServiceResultFactory.CreateError("", MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException("", exception);
            }
        }

        #endregion

        public async Task<ServiceResult<List<UserMentionDto>>> GetActivityUserListAsync(AuthenticateDto authenticate, long revisionId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateSuccess(new List<UserMentionDto>());

                var dbQuery = _documentRevisionRepository
                    .Where(a => !a.IsDeleted && a.DocumentRevisionId == revisionId && a.Document.ContractCode == authenticate.ContractCode);

                var documentGroupId = await dbQuery
                    .Select(c => c.Document.DocumentGroupId)
                    .FirstOrDefaultAsync();

                if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.Document.DocumentGroupId)))
                    return ServiceResultFactory.CreateSuccess(new List<UserMentionDto>());

                var roles = new List<string> { SCMRole.RevisionMng, SCMRole.RevisionActivityMng };
                var list = await _authenticationServices.GetAllUserHasAccessDocumentAsync(authenticate.ContractCode, roles, documentGroupId);

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

    }
}
