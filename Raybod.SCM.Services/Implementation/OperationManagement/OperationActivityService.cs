using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.DataTransferObject._PanelOperation;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Operation;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
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
    public class OperationActivityService : IOperationActivityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<Operation> _operationRepository;
        private readonly DbSet<OperationActivity> _operationActivityRepository;
        private readonly DbSet<OperationActivityTimesheet> _activityTimesheetRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyConfig _appSettings;

        public OperationActivityService(IUnitOfWork unitOfWork,
            IWebHostEnvironment hostingEnvironmentRoot,
            IOptions<CompanyAppSettingsDto> appSettings,
            IHttpContextAccessor httpContextAccessor,
            ITeamWorkAuthenticationService authenticationServices,
            ISCMLogAndNotificationService scmLogAndNotificationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationServices = authenticationServices;
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _userRepository = _unitOfWork.Set<User>();
            _operationRepository = _unitOfWork.Set<Operation>();
            _operationActivityRepository = _unitOfWork.Set<OperationActivity>();
            _activityTimesheetRepository = _unitOfWork.Set<OperationActivityTimesheet>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }


        #region OperationActivities

        public async Task<ServiceResult<OperationActivityDto>> AddOperationActivityAsync(AuthenticateDto authenticate, Guid operationId, AddOperationActivityDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.AccessDenied);

                var dbQuery = _operationRepository
                    .Where(a => !a.IsDeleted &&
                    a.IsActive &&
                    a.OperationId == operationId &&
                    a.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.EntityDoesNotExist);

                if (permission.OperationGroupList.Any() && !dbQuery.Any(a => permission.OperationGroupList.Contains(a.OperationGroupId)))
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.AccessDenied);

                var operation = await _operationRepository.Include(a=>a.OperationStatuses).Where(a => a.OperationId == operationId).FirstOrDefaultAsync();
                if (operation.OperationStatuses.Last().OperationStatus==OperationStatus.Confirmed)
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.OperationConfirmAlready);

                var ownerUserModel = await _userRepository.FirstOrDefaultAsync(a => a.Id == model.ActivityOwnerId);
                if (ownerUserModel == null)
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.UserNotExist);

                //var dActivityRole = new List<string> { SCMRole.RevisionActivityMng, SCMRole.RevisionMng };
                //var accessUserIds = await _authenticationServices
                //    .GetAllUserHasAccessDocumentAsync(authenticate.ContractCode, dActivityRole, documentRevisionModel.Document.DocumentGroupId);

                if (string.IsNullOrEmpty(model.Description) || model.ActivityOwnerId <= 0)
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.InputDataValidationError);

                //if (accessUserIds == null || !accessUserIds.Any() || !accessUserIds.Any(v => v.Id == model.ActivityOwnerId))
                //    return ServiceResultFactory.CreateError<BaseOperationActivityDto>(null, MessageId.DataInconsistency);
              

                var operatoionActivityModel = new OperationActivity
                {
                    DateEnd = model.DateEnd.UnixTimestampToDateTime(),
                    Description = model.Description,
                    ActivityOwnerId = model.ActivityOwnerId,
                    OperationId = operationId,
                    OperationActivityStatus = OperationActivityStatus.ToDo,
                    Weight =  model.Weight 
                };

                _operationActivityRepository.Add(operatoionActivityModel);
               var progressPercent= await UpdateOperationProgress(operationId, OperationType.AddOperation, operatoionActivityModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new OperationActivityDto
                    {
                        ActivityOwnerId = operatoionActivityModel.ActivityOwnerId,
                        DateEnd = operatoionActivityModel.DateEnd.ToUnixTimestamp(),
                        Description = operatoionActivityModel.Description,
                        Duration = $"{Math.Floor(operatoionActivityModel.Duration)}:{TimeSpan.FromHours(operatoionActivityModel.Duration).Minutes}",
                        OperationActivityId = operatoionActivityModel.OperationActivityId,
                        OperationActivityStatus = operatoionActivityModel.OperationActivityStatus,
                        Weight = operatoionActivityModel.Weight,
                        ProgressPercent = 0,
                        ActivityOwner = new UserAuditLogDto
                        {
                            AdderUserId=ownerUserModel.Id,
                            AdderUserName = ownerUserModel.FullName,
                            AdderUserImage = ownerUserModel.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + ownerUserModel.Image : ""
                        },
                        OperationProgressPercent =progressPercent
                    };

                    await SendNotificationAndTaskOnAddActivityAsync(authenticate, await dbQuery.FirstOrDefaultAsync(), operatoionActivityModel);

                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<OperationActivityDto>(null, exception);
            }
        }
        public async Task<ServiceResult<double>> DeleteOperationActivityAsync(AuthenticateDto authenticate, Guid operationId, long operationActivityId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError((double)0, MessageId.AccessDenied);

                var dbQuery = _operationActivityRepository
                    .Where(a => a.OperationActivityId == operationActivityId &&
                     !a.Operation.IsDeleted &&
                     a.OperationId == operationId &&
                     a.Operation.IsActive &&
                     a.Operation.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError((double)-1, MessageId.EntityDoesNotExist);

                if (permission.OperationGroupList.Any() && !dbQuery.Any(a => permission.OperationGroupList.Contains(a.Operation.OperationGroupId)))
                    return ServiceResultFactory.CreateError((double)0, MessageId.AccessDenied);


                var operationActivityModel = await dbQuery
                    .Include(a => a.OperationActivityTimesheets)
                     .Include(a => a.Operation)
                    .FirstOrDefaultAsync();


                if (operationActivityModel == null)
                    return ServiceResultFactory.CreateError((double)-1, MessageId.EntityDoesNotExist);

                if (operationActivityModel.OperationActivityStatus == OperationActivityStatus.Done)
                    return ServiceResultFactory.CreateError((double)-1, MessageId.OperationActivityHasDone);

                if (operationActivityModel.OperationActivityTimesheets.Any(a=>!a.IsDeleted))
                    return ServiceResultFactory.CreateError((double)-1, MessageId.DeleteDontAllowedBeforeSubset);
                operationActivityModel.IsDeleted = true;

                foreach (var item in operationActivityModel.OperationActivityTimesheets)
                {
                    item.IsDeleted = true;
                }
               var progressPercent= await UpdateOperationProgress(operationId,OperationType.DeleteOperation,operationActivityModel.OperationActivityId);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(progressPercent);
                }

                return ServiceResultFactory.CreateError((double)-1, MessageId.DeleteEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException((double)-1, exception);
            }
        }

        public async Task<ServiceResult<OperationActivityDto>> EditOperationActivityAsync(AuthenticateDto authenticate, Guid operationId, long operationActivityId, AddOperationActivityDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.AccessDenied);
                bool updateOperationProgress = false;
                var dbQuery = _operationActivityRepository
                  .Where(a =>!a.IsDeleted &&
                  a.OperationActivityId == operationActivityId &&
                   !a.Operation.IsDeleted &&
                   a.OperationId == operationId &&
                   a.Operation.IsActive &&
                   a.Operation.ContractCode == authenticate.ContractCode);


                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.EntityDoesNotExist);

                if (permission.OperationGroupList.Any() && !dbQuery.Any(a => permission.OperationGroupList.Contains(a.Operation.OperationGroupId)))
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.AccessDenied);

                var operationActivityModel = await dbQuery
                    .Include(a => a.ActivityOwner)
                    .Include(a => a.Operation)
                    .Include(a => a.OperationActivityTimesheets)
                    .FirstOrDefaultAsync();

                if (operationActivityModel == null)
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.EntityDoesNotExist);

                if (operationActivityModel.OperationActivityStatus == OperationActivityStatus.Done)
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.OperationActivityHasDone);

                if (string.IsNullOrEmpty(model.Description) || model.ActivityOwnerId <= 0)
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.InputDataValidationError);


                if (model.ActivityOwnerId != operationActivityModel.ActivityOwnerId)
                {
                    var ownerUserModel = await _userRepository.FirstOrDefaultAsync(a => a.Id == model.ActivityOwnerId);
                    if (ownerUserModel == null)
                        return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.UserNotExist);

                    operationActivityModel.ActivityOwnerId = model.ActivityOwnerId;
                    operationActivityModel.ActivityOwner = ownerUserModel;
                }

                //var dActivityRole = new List<string> { SCMRole.RevisionActivityMng, SCMRole.RevisionMng };
                //var accessUserIds = await _authenticationServices
                //    .GetAllUserHasAccessDocumentAsync(authenticate.ContractCode, dActivityRole, revisionActivityModel.DocumentRevision.Document.DocumentGroupId);


                //if (accessUserIds == null || !accessUserIds.Any() || !accessUserIds.Any(v => v.Id == model.ActivityOwnerId))
                //    return ServiceResultFactory.CreateError<BaseRevisionAvtivityDto>(null, MessageId.DataInconsistency);
                double progressPercent = -1;
                operationActivityModel.DateEnd = model.DateEnd.UnixTimestampToDateTime();
                operationActivityModel.Description = model.Description;
                updateOperationProgress = ( model.Weight != operationActivityModel.Weight);
                operationActivityModel.Weight = model.Weight;
                if (updateOperationProgress)
                   progressPercent= await UpdateOperationProgress(operationId, OperationType.EditOperation, operationActivityModel);
                else
                  progressPercent=  await UpdateOperationProgress(operationId, OperationType.CalculateProgress, null);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    //await _scmLogAndNotificationService.RemoveNotificationAsync(authenticate.ContractCode, revisionActivityModel.RevisionActivityId.ToString(), NotifEvent.AddRevisionActivity);


                    

                    var result = new OperationActivityDto
                    {
                        DateEnd = operationActivityModel.DateEnd.ToUnixTimestamp(),
                        Description = operationActivityModel.Description,
                        Duration = $"{Math.Floor(operationActivityModel.Duration)}:{TimeSpan.FromHours(operationActivityModel.Duration).Minutes}",
                        OperationActivityId = operationActivityModel.OperationActivityId,
                        OperationActivityStatus = operationActivityModel.OperationActivityStatus,
                        Weight = operationActivityModel.Weight,
                        ProgressPercent = operationActivityModel.OperationActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent),
                        ActivityOwner = new UserAuditLogDto
                        {
                            AdderUserId=operationActivityModel.ActivityOwner.Id,
                            AdderUserName = operationActivityModel.ActivityOwner.FullName,
                            AdderUserImage = operationActivityModel.ActivityOwner.Image != null ?
                            _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + operationActivityModel.ActivityOwner.Image : ""
                        },
                        OperationProgressPercent=progressPercent
                    };
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.EditEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<OperationActivityDto>(null, exception);
            }
        }

        public async Task<ServiceResult<OperationActivityDto>> SetOperationActivityStatusAsync(AuthenticateDto authenticate, Guid operationId, long operationActivityId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.AccessDenied);

                var dbQuery = _operationActivityRepository
                    .Where(a => a.OperationActivityId == operationActivityId &&
                    !a.IsDeleted&&
                    !a.Operation.IsDeleted &&
                     a.OperationId == operationId &&
                     a.Operation.IsActive &&
                     a.Operation.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.EntityDoesNotExist);

                if (!dbQuery.Any(a => a.ActivityOwnerId == authenticate.UserId))
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.AccessDenied);

                var operation = await _operationRepository.Include(a => a.OperationStatuses).Where(a => a.OperationId == operationId).FirstOrDefaultAsync();
                if (operation.OperationStatuses.Last().OperationStatus == OperationStatus.Confirmed)
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.OperationConfirmAlready);

                if (permission.OperationGroupList.Any() && !dbQuery.Any(a => permission.OperationGroupList.Contains(a.Operation.OperationGroupId)))
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.AccessDenied);


                var operationActivityModel = await dbQuery
                    .Include(a => a.OperationActivityTimesheets)
                    .Include(a => a.Operation)
                    .Include(a=>a.ActivityOwner)
                    .FirstOrDefaultAsync();

                if (operationActivityModel == null)
                    return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.EntityDoesNotExist);

                if (operationActivityModel.OperationActivityStatus == OperationActivityStatus.ToDo)
                {
                    operationActivityModel.OperationActivityStatus = OperationActivityStatus.Done;
                    if (operationActivityModel.OperationActivityTimesheets != null && operationActivityModel.OperationActivityTimesheets.Any() && operationActivityModel.OperationActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) < 100)
                    {
                        operationActivityModel.OperationActivityTimesheets.Add(new OperationActivityTimesheet { Duration = TimeSpan.Parse("00"), Description = "Complete Activites", DateIssue = DateTime.Now, OperationActivityId = operationActivityModel.OperationActivityId, ProgressPercent = 100 - operationActivityModel.OperationActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) });
                    }
                    else if (operationActivityModel.OperationActivityTimesheets == null || operationActivityModel.OperationActivityTimesheets.Count()==0)
                    {
                        operationActivityModel.OperationActivityTimesheets.Add(new OperationActivityTimesheet { Duration = TimeSpan.Parse("00"), Description = "Complete Activites", DateIssue = DateTime.Now, OperationActivityId = operationActivityModel.OperationActivityId, ProgressPercent = 100 });
                    }
                    
                    
                    
                    
                }

                else
                {
                    operationActivityModel.OperationActivityStatus = OperationActivityStatus.ToDo;
                    if (operationActivityModel.OperationActivityTimesheets != null && operationActivityModel.OperationActivityTimesheets.Any(a => !a.IsDeleted && a.Description == "Complete Activites"))
                    {
                        operationActivityModel.OperationActivityTimesheets.Where(a => !a.IsDeleted && a.Description == "Complete Activites").First().IsDeleted = true;
                    }
                    
                }
               var progressPercent= await UpdateOperationProgress(operationId, OperationType.SetOperationActivityStatus, operationActivityModel);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await LogAndTaskconfigOnSetOperationStatusAsync(authenticate, operationActivityModel);
                    var result = new OperationActivityDto
                    {
                        ActivityOwnerId = operationActivityModel.ActivityOwnerId,
                        DateEnd = operationActivityModel.DateEnd.ToUnixTimestamp(),
                        Description = operationActivityModel.Description,
                        Duration = $"{Math.Floor(operationActivityModel.Duration)}:{TimeSpan.FromHours(operationActivityModel.Duration).Minutes}",
                        OperationActivityId = operationActivityModel.OperationActivityId,
                        OperationActivityStatus = operationActivityModel.OperationActivityStatus,
                        Weight = operationActivityModel.Weight,
                        ProgressPercent = operationActivityModel.OperationActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent),
                        ActivityOwner = new UserAuditLogDto
                        {
                            AdderUserName = operationActivityModel.ActivityOwner.FullName,
                            AdderUserImage = operationActivityModel.ActivityOwner.Image != null ?
                           _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + operationActivityModel.ActivityOwner.Image : ""
                        },
                        OperationProgressPercent = progressPercent
                    };
                    return ServiceResultFactory.CreateSuccess(result);
                }

                return ServiceResultFactory.CreateError<OperationActivityDto>(null, MessageId.DeleteEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<OperationActivityDto>(null, exception);
            }
        }

        #endregion

        #region TimeSheet

        public async Task<ServiceResult<ResultAddActivityTimeSheetDto>> AddActivityTimeSheetAsync(AuthenticateDto authenticate, Guid operationId, long revisionActivityId, AddActivityTimeSheetDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.AccessDenied);

                var dbQuery = _operationActivityRepository.Where(a =>
                !a.IsDeleted&&
                    !a.Operation.IsDeleted &&
                    a.Operation.IsActive &&
                    a.OperationActivityId == revisionActivityId &&
                    a.OperationId == operationId &&
                    a.Operation.ContractCode == authenticate.ContractCode);

               

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.EntityDoesNotExist);

                if (!dbQuery.Any(a => a.ActivityOwnerId == authenticate.UserId))
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.AccessDenied);


                if (permission.OperationGroupList.Any() && !dbQuery.Any(a => permission.OperationGroupList.Contains(a.Operation.OperationGroupId)))
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.AccessDenied);

                var operationActivityModel = await dbQuery
                    .Include(a => a.OperationActivityTimesheets)
                    .Include(a => a.Operation)
                    .FirstOrDefaultAsync();

                if (operationActivityModel == null)
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.EntityDoesNotExist);

                if (operationActivityModel.OperationActivityStatus == OperationActivityStatus.Done)
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.OperationActivityHasDone);

                if (operationActivityModel.OperationActivityTimesheets != null && operationActivityModel.OperationActivityTimesheets.Any() && model.ProgressPercent != null)
                {
                    if (operationActivityModel.OperationActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) + model.ProgressPercent > 100)
                        return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.ProgressPercentSumCannotBeOver100);
                }

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

                var activityTimeSheetModel = new OperationActivityTimesheet
                {
                    DateIssue =model.DateIssue!=null? model.DateIssue.Value.UnixTimestampToDateTime():DateTime.Now,
                    Description = model.Description,
                    OperationActivityId = operationActivityModel.OperationActivityId,
                    Duration = ts,
                    ProgressPercent = model.ProgressPercent != null ? model.ProgressPercent.Value : 0
                };

                if (operationActivityModel.OperationActivityTimesheets != null && operationActivityModel.OperationActivityTimesheets.Any())
                    operationActivityModel.OperationActivityTimesheets.Add(activityTimeSheetModel);
                else
                {
                    operationActivityModel.OperationActivityTimesheets = new List<OperationActivityTimesheet>();
                    operationActivityModel.OperationActivityTimesheets.Add(activityTimeSheetModel);
                }

                var sumDurationTimeSpan = new TimeSpan(operationActivityModel.OperationActivityTimesheets.Where(a => !a.IsDeleted).Sum(v => v.Duration.Ticks));
                operationActivityModel.Duration = sumDurationTimeSpan.TotalHours;
               var progressPercent= await UpdateOperationProgress(operationId,OperationType.AddActivityTimeSheet, operationActivityModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                  
                    var result = new ResultAddActivityTimeSheetDto
                    {
                        TotalDuration = $"{Math.Floor(sumDurationTimeSpan.TotalHours)}:{sumDurationTimeSpan.Minutes}",
                        ActivityTimesheetId = activityTimeSheetModel.ActivityTimesheetId,
                        DateIssue = activityTimeSheetModel.DateIssue.ToUnixTimestamp(),
                        Description = activityTimeSheetModel.Description,
                        Duration = activityTimeSheetModel.Duration.ToString(@"hh\:mm"),
                        ProgressPercent = activityTimeSheetModel.ProgressPercent,
                        UserAudit = new UserAuditLogDto
                        {
                            
                            AdderUserName = authenticate.UserFullName,
                            AdderUserImage = authenticate.UserImage != null ? authenticate.UserImage : ""
                        },
                        ActivityProgressPercent= _activityTimesheetRepository.Where(a=>!a.IsDeleted&&a.OperationActivityId==operationActivityModel.OperationActivityId).Sum(c=>c.ProgressPercent),
                        OperationProgressPercent= progressPercent
                    };


                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ResultAddActivityTimeSheetDto>(null, exception);
            }
        }

        public async Task<ServiceResult<ActivityTimeSheetDto>> DeleteActivityTimeSheetAsync(AuthenticateDto authenticate, Guid operationId, long operationActivityId, long activityTimeSheetId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ActivityTimeSheetDto>(null, MessageId.AccessDenied);

                var dbQuery = _activityTimesheetRepository.Where(a =>
                    !a.IsDeleted &&
                    a.OperationActivity.Operation.IsActive &&
                    a.ActivityTimesheetId == activityTimeSheetId &&
                    !a.OperationActivity.Operation.IsDeleted &&
                    a.OperationActivity.OperationActivityId == operationActivityId &&
                    a.OperationActivity.OperationId == operationId &&
                    a.OperationActivity.Operation.ContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<ActivityTimeSheetDto>(null, MessageId.EntityDoesNotExist);

                if (!dbQuery.Any(a => a.OperationActivity.ActivityOwnerId == authenticate.UserId))
                    return ServiceResultFactory.CreateError<ActivityTimeSheetDto>(null, MessageId.AccessDenied);

                if (permission.OperationGroupList.Any() && !dbQuery.Any(a => permission.OperationGroupList.Contains(a.OperationActivity.Operation.OperationGroupId)))
                    return ServiceResultFactory.CreateError<ActivityTimeSheetDto>(null, MessageId.AccessDenied);

                var activityTimeSheetModel = await dbQuery
                    .Include(a => a.OperationActivity)
                    .ThenInclude(a => a.Operation)
                    .FirstOrDefaultAsync();

                if (activityTimeSheetModel == null)
                    return ServiceResultFactory.CreateError<ActivityTimeSheetDto>(null, MessageId.EntityDoesNotExist);

                if (activityTimeSheetModel.OperationActivity.OperationActivityStatus == OperationActivityStatus.Done)
                    return ServiceResultFactory.CreateError<ActivityTimeSheetDto>(null, MessageId.OperationActivityHasDone);

                activityTimeSheetModel.IsDeleted = true;

                var totalHour = activityTimeSheetModel.Duration.TotalHours;
                activityTimeSheetModel.OperationActivity.Duration -= totalHour;
                var progressPercent=await UpdateOperationProgress(operationId,OperationType.DeleteActivityTimeSheet, activityTimeSheetModel);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new ActivityTimeSheetDto
                    {
                        TotalDuration = activityTimeSheetModel.OperationActivity.Duration.ToString(),
                        ActivityProgressPercent = _activityTimesheetRepository.Where(a => !a.IsDeleted && a.OperationActivityId == operationActivityId).Sum(c => c.ProgressPercent),
                        OperationProgressPercent = progressPercent,
                    };
                    return ServiceResultFactory.CreateSuccess(result);
                }

                return ServiceResultFactory.CreateError<ActivityTimeSheetDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<ActivityTimeSheetDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<ListActivityTimeSheetDto>>> GetActivityTimeSheetAsync(AuthenticateDto authenticate, Guid operationId, long operationActivityId)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<List<ListActivityTimeSheetDto>>(null, MessageId.AccessDenied);

                var dbQuery = _operationActivityRepository
                    .AsNoTracking()
                    .Where(a =>!a.IsDeleted && !a.Operation.IsDeleted &&
                    a.OperationId == operationId &&
                    a.OperationActivityId == operationActivityId &&
                    a.Operation.IsActive &&
                    a.Operation.ContractCode == authenticate.ContractCode);


                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<ListActivityTimeSheetDto>>(null, MessageId.EntityDoesNotExist);

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                //    return ServiceResultFactory.CreateError<List<ListActivityTimeSheetDto>>(null, MessageId.AccessDenied);

                var result = await dbQuery.SelectMany(c => c.OperationActivityTimesheets.Where(v => !v.IsDeleted))
                    .Select(v => new ListActivityTimeSheetDto
                    {
                        ActivityTimesheetId = v.ActivityTimesheetId,
                        DateIssue = v.DateIssue.ToUnixTimestamp(),
                        Description = v.Description,
                        Duration = v.Duration.ToString(@"hh\:mm"),
                        ProgressPercent = v.ProgressPercent,
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
        public async Task<ServiceResult<List<BaseOperationActivityDto>>> GetOperationActivityListAsync(AuthenticateDto authenticate, Guid operationId)
        {
            try
            {
                //var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<List<ListActivityTimeSheetDto>>(null, MessageId.AccessDenied);

                var dbQuery = _operationActivityRepository
                    .AsNoTracking()
                    .Where(a =>!a.IsDeleted &&
                    !a.Operation.IsDeleted &&
                    a.OperationId == operationId &&
                    a.Operation.IsActive &&
                    a.Operation.ContractCode == authenticate.ContractCode);


                

                //if (permission.DocumentGroupIds.Any() && !dbQuery.Any(a => permission.DocumentGroupIds.Contains(a.DocumentRevision.Document.DocumentGroupId)))
                //    return ServiceResultFactory.CreateError<List<ListActivityTimeSheetDto>>(null, MessageId.AccessDenied);

                var result = await dbQuery.Select(c => new BaseOperationActivityDto
                    {
                        ActivityOwner = c.ActivityOwner != null ?
                        new UserAuditLogDto
                        {
                            AdderUserId=c.ActivityOwner.Id,
                            AdderUserName = c.ActivityOwner.FullName,
                            AdderUserImage = c.ActivityOwner.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.ActivityOwner.Image : ""
                        } : null,
                        DateEnd = c.DateEnd.ToUnixTimestamp(),
                        Description = c.Description,
                        Duration = $"{Math.Floor(c.Duration)}:{TimeSpan.FromHours(c.Duration).Minutes}",
                        OperationActivityId = c.OperationActivityId,
                        OperationActivityStatus = c.OperationActivityStatus,
                        Weight = c.Weight,
                        ProgressPercent = _activityTimesheetRepository.Where(d => !d.IsDeleted && d.OperationActivityId == c.OperationActivityId).Sum(d => d.ProgressPercent)


                    }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<BaseOperationActivityDto>>(null, exception);
            }



        }
        #endregion

        public async Task<ServiceResult<List<UserMentionDto>>> GetActivityUserListAsync(AuthenticateDto authenticate, Guid operationId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermission(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateSuccess(new List<UserMentionDto>());

                var dbQuery = _operationRepository
                    .Where(a => a.IsActive && !a.IsDeleted && a.OperationId == operationId && a.ContractCode == authenticate.ContractCode);

                var operaitonGroupId = await dbQuery
                    .Select(c => c.OperationGroupId)
                    .FirstOrDefaultAsync();

                if (permission.OperationGroupList.Any() && !dbQuery.Any(a => permission.OperationGroupList.Contains(a.OperationGroupId)))
                    return ServiceResultFactory.CreateSuccess(new List<UserMentionDto>());


                var list = await _authenticationServices.GetAllUserHasAccessOperationActivityAsync(authenticate.ContractCode,operaitonGroupId);

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
        private async Task<double> UpdateOperationProgress(Guid operationId,OperationType type,object model)
        {
            double result = 0;
            var dbQuery = _operationRepository
                 .Include(a => a.OperationStatuses)
                 .Include(a => a.OperationProgresses)
                 .Where(a => !a.IsDeleted && a.OperationId == operationId);
            if (dbQuery == null)
                return -1;
            else if (type == OperationType.CalculateProgress)
            {
                var operation = await dbQuery.FirstOrDefaultAsync();
               return operation.OperationProgresses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault().ProgressPercent;
            }
            else
            {
                double average = 0;
                double weight = 0;
                var operation = await dbQuery.FirstOrDefaultAsync();
                var operationActivity = await _operationActivityRepository.Include(a => a.OperationActivityTimesheets).Where(a => !a.IsDeleted && a.OperationId == operationId).ToListAsync();
                if (type == OperationType.DeleteOperation)
                {
                    foreach (var item in operationActivity.Where(a=>a.OperationActivityId!=(long)model))
                    {

                        if (item.OperationActivityTimesheets != null && item.OperationActivityTimesheets.Any())
                            average += item.OperationActivityTimesheets.Where(a=>!a.IsDeleted ).Sum(a => a.ProgressPercent) * item.Weight;
                        weight += item.Weight;
                    }
                }
                else if (type == OperationType.EditOperation)
                {
                    var operationModel = model as OperationActivity;
                    foreach (var item in operationActivity.Where(a => a.OperationActivityId!=operationModel.OperationActivityId))
                    {

                        if (item.OperationActivityTimesheets != null && item.OperationActivityTimesheets.Any())
                            average += item.OperationActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * item.Weight;
                        weight += item.Weight;
                    }
                    foreach(var item in operationActivity.Where(a => a.OperationActivityId == operationModel.OperationActivityId))
                    {
                        if (item.OperationActivityTimesheets != null && item.OperationActivityTimesheets.Any())
                            average += item.OperationActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * operationModel.Weight;
                        weight += operationModel.Weight;
                    }
                }
                else if (type == OperationType.AddOperation)
                {
                    var operationModel = model as OperationActivity;
                    foreach (var item in operationActivity)
                    {

                        if (item.OperationActivityTimesheets != null && item.OperationActivityTimesheets.Any())
                            average += item.OperationActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * item.Weight;
                        weight += item.Weight;
                    }

                        weight += operationModel.Weight;

                }
                else if (type == OperationType.SetOperationActivityStatus)
                {
                    var operationModel = model as OperationActivity;
                    foreach (var item in operationActivity.Where(a => a.OperationActivityId != operationModel.OperationActivityId))
                    {

                        if (item.OperationActivityTimesheets != null && item.OperationActivityTimesheets.Any())
                            average += item.OperationActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * item.Weight;
                        weight += item.Weight;
                    }
                   
                        if (operationModel.OperationActivityTimesheets != null && operationModel.OperationActivityTimesheets.Any())
                            average += operationModel.OperationActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * operationModel.Weight;
                    weight += operationModel.Weight;

                }
                else if (type == OperationType.AddActivityTimeSheet)
                {
                    var operationModel = model as OperationActivity;
                    foreach (var item in operationActivity.Where(a => a.OperationActivityId != operationModel.OperationActivityId))
                    {

                        if (item.OperationActivityTimesheets != null && item.OperationActivityTimesheets.Any())
                            average += item.OperationActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * item.Weight;
                        weight += item.Weight;
                    }

                    if (operationModel.OperationActivityTimesheets != null && operationModel.OperationActivityTimesheets.Any())
                        average += operationModel.OperationActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * operationModel.Weight;
                    weight += operationModel.Weight;
                }
                else if (type == OperationType.DeleteActivityTimeSheet)
                {
                    var operationModel = model as OperationActivityTimesheet;
                    foreach (var item in operationActivity)
                    {

                        if (item.OperationActivityTimesheets != null && item.OperationActivityTimesheets.Any())
                            average += item.OperationActivityTimesheets.Where(a=>a.ActivityTimesheetId!=operationModel.ActivityTimesheetId&&!a.IsDeleted).Sum(a => a.ProgressPercent) * item.Weight;
                        weight += item.Weight;
                    }

                   

                }
                if (operation.OperationProgresses != null && operation.OperationProgresses.Any())
                {
                    if (operationActivity != null && operationActivity.Any())
                    {

                        operation.OperationProgresses.Add(new OperationProgress { IsDeleted = false, ProgressPercent =(weight>0)? average / weight:0 });
                        result =(weight>0)? average / weight:0;
                    }
                    else
                    {
                        operation.OperationProgresses.Add(new OperationProgress { IsDeleted = false, ProgressPercent = 0 });
                        result = 0;
                    }
                }

                else
                {
                    if (operationActivity != null && operationActivity.Any())
                    {
                        operation.OperationProgresses = new List<OperationProgress> { new OperationProgress { IsDeleted = false, ProgressPercent = (weight > 0) ? average / weight : 0 } };
                        result =(weight>0)? average / weight:0;
                    }

                    else
                    {
                        operation.OperationProgresses.Add(new OperationProgress { IsDeleted = false, ProgressPercent = 0 });
                        result = 0;
                    }
                }



                return result;
            }

        }
        private async Task SendNotificationAndTaskOnAddActivityAsync(AuthenticateDto authenticate, Operation operationModel, OperationActivity operationActivityModel)
        {
            var activityUserIds = await GetOperationActivityUserIdsAsync(operationModel.OperationId, operationActivityModel.AdderUserId.Value);

            var logModel = new AddAuditLogDto
            {
                ContractCode = authenticate.ContractCode,
                Description = operationModel.OperationDescription,
                Message = operationActivityModel.Description,
                FormCode = operationModel.OperationCode,
                KeyValue = operationActivityModel.OperationActivityId.ToString(),
                NotifEvent = NotifEvent.AddOperationActivity,
                RootKeyValue = operationModel.OperationId.ToString(),
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName,
                OperationGroupId = operationModel.OperationGroupId,
                ReceiverLogUserIds = activityUserIds
            };

            var taskModel = new AddTaskNotificationDto
            {
                ContractCode = authenticate.ContractCode,
                Description = operationModel.OperationDescription,
                Message = operationActivityModel.Description,
                FormCode = operationModel.OperationCode,
                Quantity = operationActivityModel.DateEnd.ToUnixTimestamp().ToString(),
                KeyValue = operationActivityModel.OperationActivityId.ToString(),
                NotifEvent = NotifEvent.AddOperationActivity,
                RootKeyValue = operationModel.OperationId.ToString(),
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName,
                Users = new List<int> { operationActivityModel.ActivityOwnerId }
            };

            var res1 = await _scmLogAndNotificationService.AddScmAuditLogAndTaskAsync(logModel, taskModel);
        }
        private async Task<List<int>> GetOperationActivityUserIdsAsync(Guid operationId, int operationCreaterUserId)
        {
            var userIds = await _operationActivityRepository
                      .Where(a => !a.IsDeleted &&
                      a.OperationId == operationId)
                      .Select(c => c.ActivityOwnerId)
                      .ToListAsync();

            userIds.Add(operationCreaterUserId);

            return userIds.Distinct().ToList();
        }

        private async Task LogAndTaskconfigOnSetOperationStatusAsync(AuthenticateDto authenticate, OperationActivity operationActivityModel)
        {
            await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, operationActivityModel.OperationActivityId.ToString(), NotifEvent.AddOperationActivity);

            List<int> activityUserIds = await GetOperationActivityUserIdsAsync(operationActivityModel.OperationId, operationActivityModel.Operation.AdderUserId.Value);

            if (operationActivityModel.OperationActivityStatus == OperationActivityStatus.Done)
                await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                {
                    ContractCode = authenticate.ContractCode,
                    Description = operationActivityModel.Operation.OperationDescription,
                    Message = operationActivityModel.Description,
                    FormCode = operationActivityModel.Operation.OperationCode,
                    KeyValue = operationActivityModel.OperationActivityId.ToString(),
                    NotifEvent = NotifEvent.OperationActivityDone,
                    RootKeyValue = operationActivityModel.OperationId.ToString(),
                    PerformerUserId = authenticate.UserId,
                    PerformerUserFullName = authenticate.UserFullName,
                    OperationGroupId = operationActivityModel.Operation.OperationGroupId,
                    ReceiverLogUserIds = activityUserIds
                }, null);
        }

        
    }
    public enum OperationType
    {
        DeleteOperation=1,
        EditOperation=2,
        SetOperationActivityStatus=3,
        AddActivityTimeSheet=4,
        DeleteActivityTimeSheet=5,
        AddOperation=6,
        CalculateProgress=7
    }
}
