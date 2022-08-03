using Exon.TheWeb.Service.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.PO.POActivity;
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
    public class POActivityService : IPOActivityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<PO> _poRepository;
        private readonly DbSet<POActivity> _poActivityRepository;
        private readonly DbSet<POActivityTimesheet> _activityTimesheetRepository;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly CompanyAppSettingsDto _appSettings;

        public POActivityService(IUnitOfWork unitOfWork,
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
            _poRepository = _unitOfWork.Set<PO>();
            _poActivityRepository = _unitOfWork.Set<POActivity>();
            _activityTimesheetRepository = _unitOfWork.Set<POActivityTimesheet>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
        }

        #region PO activity
        public async Task<ServiceResult<BasePOAvtivityDto>> AddPOActivityAsync(AuthenticateDto authenticate, long poId,
            AddPOActivityDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.AccessDenied);

                var dbQuery = _poRepository
                    .Where(a => !a.IsDeleted &&
                    a.POId == poId &&
                    !a.IsDeleted &&
      
                    a.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.AccessDenied);

                var POModel = await dbQuery
                    .Include(a => a.Supplier)
                    .FirstOrDefaultAsync();

                if (POModel == null)
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.EntityDoesNotExist);
                if(POModel.POStatus==POStatus.Canceled)
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.CantDoneBecausePOCanceled);
                var ownerUserModel = await _userRepository.FirstOrDefaultAsync(a => a.Id == model.ActivityOwnerId);
                if (ownerUserModel == null)
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.UserNotExist);

                var dActivityRole = new List<string> { SCMRole.POActivityOwner, SCMRole.POActivityMng };
                var accessUserIds = await _authenticationServices
                    .GetAllUserHasAccessPOAsync(authenticate.ContractCode, dActivityRole, POModel.ProductGroupId);

                if (string.IsNullOrEmpty(model.Description) || model.ActivityOwnerId <= 0)
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.InputDataValidationError);

                if (accessUserIds == null || !accessUserIds.Any() || !accessUserIds.Any(v => v.Id == model.ActivityOwnerId))
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.DataInconsistency);

                var POActivityModel = new POActivity
                {
                    DateEnd = model.DateEnd.UnixTimestampToDateTime(),
                    Description = model.Description,
                    ActivityOwnerId = model.ActivityOwnerId,
                    POId = poId,
                    Weight=model.Weight,
                    ActivityStatus = POActivityStatus.Todo,
                };

                _poActivityRepository.Add(POActivityModel);
                var progressPercent = await UpdatePoProgress(poId, PoActivityType.AddActivity, POActivityModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new BasePOAvtivityDto
                    {
                        ActivityOwnerId = POActivityModel.ActivityOwnerId,
                        DateEnd = POActivityModel.DateEnd.ToUnixTimestamp(),
                        Description = POActivityModel.Description,
                        Duration = $"{Math.Floor(POActivityModel.Duration)}:{TimeSpan.FromHours(POActivityModel.Duration).Minutes}",
                        POActivityId = POActivityModel.POActivityId,
                        ActivityStatus = POActivityModel.ActivityStatus,
                        ActivityOwner = new UserAuditLogDto
                        {
                            AdderUserName = ownerUserModel.FullName,
                            AdderUserImage = ownerUserModel.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + ownerUserModel.Image : ""
                        },
                        ProgressPercent = 0,
                        PoProgressPercent = progressPercent

                    };

                    await SendNotificationAndTaskOnAddActivityAsync(authenticate, POModel, POActivityModel);

                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.AddEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BasePOAvtivityDto>(null, exception);
            }
        }

        private async Task SendNotificationAndTaskOnAddActivityAsync(AuthenticateDto authenticate, PO poModel, POActivity poActivityModel)
        {
            var activityUserIds = await GetPOActivityUserIdsAsync(poModel.POId, poModel.AdderUserId.Value);

            var logModel = new AddAuditLogDto
            {
                ContractCode = authenticate.ContractCode,
                Description = poModel.Supplier.Name,
                KeyValue = poActivityModel.POActivityId.ToString(),
                RootKeyValue = poModel.POId.ToString(),
                Message = poActivityModel.Description,
                FormCode = poModel.POCode,
                Temp = poActivityModel.DateEnd != null ? poActivityModel.DateEnd.ToPersianDateString() : "",
                NotifEvent = NotifEvent.AddPOActivity,
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName,
                ProductGroupId = poModel.ProductGroupId,
                ReceiverLogUserIds = activityUserIds,
            };

            var taskModel = new AddTaskNotificationDto
            {
                ContractCode = authenticate.ContractCode,
                Description = poModel.Supplier.Name,
                Message = poActivityModel.Description,
                Temp = poActivityModel.DateEnd != null ? poActivityModel.DateEnd.ToPersianDateString() : "",
                FormCode = poModel.POCode,
                Quantity = poActivityModel.DateEnd.ToUnixTimestamp().ToString(),
                KeyValue = poActivityModel.POActivityId.ToString(),
                NotifEvent = NotifEvent.AddPOActivity,
                RootKeyValue = poModel.POId.ToString(),
                PerformerUserId = authenticate.UserId,
                PerformerUserFullName = authenticate.UserFullName,
                Users = new List<int> { poActivityModel.ActivityOwnerId }
            };

            var res1 = await _scmLogAndNotificationService.AddScmAuditLogAndTaskAsync(logModel, taskModel);
        }

        public async Task<ServiceResult<BasePOAvtivityDto>> SetActivityStatusAsync(AuthenticateDto authenticate, long poId, long poActivityId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.AccessDenied);

                var dbQuery = _poActivityRepository
                    .Where(a => a.POActivityId == poActivityId &&
                    !a.PO.IsDeleted &&
                     a.POId == poId &&
                     a.PO.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.EntityDoesNotExist);
                if (dbQuery.Any(a=>a.PO.POStatus==POStatus.Canceled) )
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.CantDoneBecausePOCanceled);

                if (!dbQuery.Any(a => a.ActivityOwnerId == authenticate.UserId))
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.AccessDenied);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.AccessDenied);
               

                var poActivityModel = await dbQuery
                    .Include(a => a.ActivityTimesheets)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.Supplier)
                    .FirstOrDefaultAsync();
                var ownerUserModel = await _userRepository.FirstOrDefaultAsync(a => a.Id == poActivityModel.ActivityOwnerId);
                if (ownerUserModel == null)
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.UserNotExist);
                if (poActivityModel == null)
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.EntityDoesNotExist);
                if (poActivityModel.ActivityStatus == POActivityStatus.Todo)
                {
                    poActivityModel.ActivityStatus = POActivityStatus.Done;
                    if (poActivityModel.ActivityTimesheets != null && poActivityModel.ActivityTimesheets.Any() && poActivityModel.ActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) < 100)
                    {
                        poActivityModel.ActivityTimesheets.Add(new POActivityTimesheet { Duration = TimeSpan.Parse("00"), Description = "Complete Activites", DateIssue = DateTime.Now, POActivityId = poActivityModel.POActivityId, ProgressPercent = 100 - poActivityModel.ActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) });
                    }
                    else if (poActivityModel.ActivityTimesheets == null || poActivityModel.ActivityTimesheets.Count() == 0)
                    {
                        poActivityModel.ActivityTimesheets.Add(new POActivityTimesheet { Duration = TimeSpan.Parse("00"), Description = "Complete Activites", DateIssue = DateTime.Now, POActivityId = poActivityModel.POActivityId, ProgressPercent = 100 });
                    }
                }

                else
                {
                    poActivityModel.ActivityStatus = POActivityStatus.Todo;
                    if (poActivityModel.ActivityTimesheets != null && poActivityModel.ActivityTimesheets.Any(a => !a.IsDeleted && a.Description == "Complete Activites"))
                    {
                        poActivityModel.ActivityTimesheets.Where(a => !a.IsDeleted && a.Description == "Complete Activites").First().IsDeleted = true;
                    }

                }
                var progressPercent = await UpdatePoProgress(poId, PoActivityType.SetPoActivityStatus, poActivityModel);
               

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await LogAndTaskconfigOnSetPOStatusAsync(authenticate, poActivityModel);
                    var result = new BasePOAvtivityDto
                    {
                        ActivityOwnerId = poActivityModel.ActivityOwnerId,
                        DateEnd = poActivityModel.DateEnd.ToUnixTimestamp(),
                        Description = poActivityModel.Description,
                        Duration = $"{Math.Floor(poActivityModel.Duration)}:{TimeSpan.FromHours(poActivityModel.Duration).Minutes}",
                        POActivityId = poActivityModel.POActivityId,
                        ActivityStatus = poActivityModel.ActivityStatus,
                        ActivityOwner = new UserAuditLogDto
                        {
                            AdderUserName = ownerUserModel.FullName,
                            AdderUserImage = ownerUserModel.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + ownerUserModel.Image : ""
                        },
                        ProgressPercent = poActivityModel.ActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent),
                        PoProgressPercent = progressPercent

                    };
                    return ServiceResultFactory.CreateSuccess(result);
                }

                return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.DeleteEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BasePOAvtivityDto>(null, exception);
            }
        }

        private async Task LogAndTaskconfigOnSetPOStatusAsync(AuthenticateDto authenticate, POActivity poActivityModel)
        {
            await _scmLogAndNotificationService.SetDonedNotificationAsync(authenticate.UserId, authenticate.ContractCode, poActivityModel.POActivityId.ToString(), NotifEvent.AddPOActivity);

            List<int> activityUserIds = await GetPOActivityUserIdsAsync(poActivityModel.POId, poActivityModel.PO.AdderUserId.Value);

            if (poActivityModel.ActivityStatus == POActivityStatus.Done)
                await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                {
                    ContractCode = authenticate.ContractCode,
                    Description = poActivityModel.PO.Supplier.Name,
                    Message = poActivityModel.Description,
                    FormCode = poActivityModel.PO.POCode,
                    KeyValue = poActivityModel.POActivityId.ToString(),
                    NotifEvent = NotifEvent.POActivityDone,
                    RootKeyValue = poActivityModel.POId.ToString(),
                    PerformerUserId = authenticate.UserId,
                    PerformerUserFullName = authenticate.UserFullName,
                    ProductGroupId = poActivityModel.PO.ProductGroupId,
                    ReceiverLogUserIds = activityUserIds
                }, null);
        }

        private async Task<List<int>> GetPOActivityUserIdsAsync(long poId, int poCreaterUserId)
        {
            var userIds = await _poActivityRepository
                      .Where(a => !a.IsDeleted &&
                      a.POId == poId)
                      .Select(c => c.ActivityOwnerId)
                      .ToListAsync();

            userIds.Add(poCreaterUserId);

            return userIds.Distinct().ToList();
        }

        public async Task<ServiceResult<double>> DeletePOActivityAsync(AuthenticateDto authenticate, long poId,
            long poActivityId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError((double)-1, MessageId.AccessDenied);

                var dbQuery = _poActivityRepository
                    .Where(a => a.POActivityId == poActivityId &&
                     !a.PO.IsDeleted &&
                     a.POId == poId &&
                     a.PO.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError((double)-1, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError((double)-1, MessageId.AccessDenied);


                var poActivityModel = await dbQuery
                    .Include(a => a.ActivityTimesheets)
                     .Include(a => a.PO)
                     .ThenInclude(a => a.Supplier)
                    .FirstOrDefaultAsync();

                if (poActivityModel == null)
                    return ServiceResultFactory.CreateError((double)-1, MessageId.EntityDoesNotExist);

                poActivityModel.IsDeleted = true;

                foreach (var item in poActivityModel.ActivityTimesheets)
                {
                    item.IsDeleted = true;
                }
                var progressPercent = await UpdatePoProgress(poId, PoActivityType.DeleteActivity, poActivityModel.POActivityId);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    List<int> activityUserIds = await GetPOActivityUserIdsAsync(poActivityModel.POId, poActivityModel.PO.AdderUserId.Value);

                    var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = poActivityModel.PO.Supplier.Name,
                        Message = poActivityModel.Description,
                        FormCode = poActivityModel.PO.POCode,
                        KeyValue = poActivityModel.POActivityId.ToString(),
                        NotifEvent = NotifEvent.DeletePOActivity,
                        RootKeyValue = poActivityModel.POId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        ProductGroupId = poActivityModel.PO.ProductGroupId,
                        ReceiverLogUserIds = activityUserIds
                    }, null);
                    await _scmLogAndNotificationService.RemoveNotificationAsync(authenticate.ContractCode, poActivityModel.POActivityId.ToString(), NotifEvent.AddPOActivity);

                    return ServiceResultFactory.CreateSuccess(progressPercent);
                }

                return ServiceResultFactory.CreateError((double)-1, MessageId.DeleteEntityFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException((double)-1, exception);
            }
        }
        #endregion

        public async Task<ServiceResult<BasePOAvtivityDto>> EditPOActivityAsync(AuthenticateDto authenticate, long poId, long poActivityId, AddPOActivityDto model)
        {
            try
            {

                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.AccessDenied);

                var dbQuery = _poActivityRepository
                  .Where(a => a.POActivityId == poActivityId &&
                   !a.PO.IsDeleted &&
                   a.POId == poId &&
                   !a.IsDeleted &&
                   a.PO.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.EntityDoesNotExist);

                if (dbQuery.Any(a=>a.PO.POStatus==POStatus.Canceled))
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.CantDoneBecausePOCanceled);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.AccessDenied);

                var poActivityModel = await dbQuery
                    .Include(a => a.ActivityOwner)
                    .Include(a=>a.ActivityTimesheets)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.Supplier)
                    .FirstOrDefaultAsync();

                if (poActivityModel == null)
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.EntityDoesNotExist);

                if (string.IsNullOrEmpty(model.Description) || model.ActivityOwnerId <= 0)
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.InputDataValidationError);


                if (model.ActivityOwnerId != poActivityModel.ActivityOwnerId)
                {
                    var ownerUserModel = await _userRepository.FirstOrDefaultAsync(a => a.Id == model.ActivityOwnerId);
                    if (ownerUserModel == null)
                        return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.UserNotExist);

                    poActivityModel.ActivityOwnerId = model.ActivityOwnerId;
                    poActivityModel.ActivityOwner = ownerUserModel;
                }

                var dActivityRole = new List<string> { SCMRole.POActivityMng, SCMRole.POActivityOwner };
                var accessUserIds = await _authenticationServices
                    .GetAllUserHasAccessPOAsync(authenticate.ContractCode, dActivityRole, poActivityModel.PO.ProductGroupId);


                if (accessUserIds == null || !accessUserIds.Any() || !accessUserIds.Any(v => v.Id == model.ActivityOwnerId))
                    return ServiceResultFactory.CreateError<BasePOAvtivityDto>(null, MessageId.DataInconsistency);
                double progressPercent = -1;
                poActivityModel.DateEnd = model.DateEnd.UnixTimestampToDateTime();
                poActivityModel.Description = model.Description;
               bool updateOperationProgress = (model.Weight != poActivityModel.Weight);
                poActivityModel.Weight = model.Weight;
                if (updateOperationProgress)
                    progressPercent = await UpdatePoProgress(poId, PoActivityType.EditActivity, poActivityModel);
                else
                    progressPercent = await UpdatePoProgress(poId, PoActivityType.CalculateProgress, null);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    await _scmLogAndNotificationService.RemoveNotificationAsync(authenticate.ContractCode, poActivityModel.POActivityId.ToString(), NotifEvent.AddPOActivity);

                    List<int> activityUserIds = await GetPOActivityUserIdsAsync(poActivityModel.POId, poActivityModel.PO.AdderUserId.Value);

                    var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = poActivityModel.PO.Supplier.Name,
                        Message = poActivityModel.Description,
                        FormCode = poActivityModel.PO.POCode,
                        KeyValue = poActivityModel.POActivityId.ToString(),
                        NotifEvent = NotifEvent.EditPOActivity,
                        RootKeyValue = poActivityModel.POId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        ProductGroupId = poActivityModel.PO.ProductGroupId,
                        ReceiverLogUserIds = activityUserIds
                    }, null);
                }

                var result = new BasePOAvtivityDto
                {
                    ActivityOwnerId = poActivityModel.ActivityOwnerId,
                    DateEnd = poActivityModel.DateEnd.ToUnixTimestamp(),
                    Description = poActivityModel.Description,
                    Duration = $"{Math.Floor(poActivityModel.Duration)}:{TimeSpan.FromHours(poActivityModel.Duration).Minutes}",
                    POActivityId = poActivityModel.POActivityId,
                    ActivityStatus = poActivityModel.ActivityStatus,
                    ActivityOwner = new UserAuditLogDto
                    {
                        AdderUserId=poActivityModel.ActivityOwnerId,
                        AdderUserName = poActivityModel.ActivityOwner.FullName,
                        AdderUserImage = poActivityModel.ActivityOwner.Image != null ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + poActivityModel.ActivityOwner.Image : ""
                    },
                    ProgressPercent = poActivityModel.ActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent),
                    PoProgressPercent = progressPercent,
                    Weight= poActivityModel.Weight
                    
                    
                };
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<BasePOAvtivityDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<BasePOAvtivityDto>>> GetPOActivityListAsync(AuthenticateDto authenticate, long poId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<BasePOAvtivityDto>>(null, MessageId.AccessDenied);

                var dbQuery = _poActivityRepository
                  .Where(a => !a.PO.IsDeleted &&
                   a.POId == poId &&
                   !a.IsDeleted &&
                   a.PO.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId));

                var result = await dbQuery.Select(x => new BasePOAvtivityDto
                {
                    POActivityId = x.POActivityId,
                    ActivityOwnerId = x.ActivityOwnerId,
                    Description = x.Description,
                    ActivityStatus = x.ActivityStatus,
                    Duration = $"{Math.Floor(x.Duration)}:{TimeSpan.FromHours(x.Duration).Minutes}",
                    DateEnd = x.DateEnd.ToUnixTimestamp(),
                    ActivityOwner = new UserAuditLogDto
                    {
                        AdderUserId=x.ActivityOwner.Id,
                        AdderUserName = x.ActivityOwner.FullName,
                        AdderUserImage = x.ActivityOwner.Image != null ?
                        _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + x.ActivityOwner.Image : ""
                    },
                    Weight = x.Weight,
                    ProgressPercent = _activityTimesheetRepository.Where(d => !d.IsDeleted && d.POActivityId == x.POActivityId).Sum(d => d.ProgressPercent)
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<BasePOAvtivityDto>>(null, exception);
            }
        }

        #region PO TimeSheet
        public async Task<ServiceResult<ResultAddActivityTimeSheetDto>> AddActivityTimeSheetAsync(AuthenticateDto authenticate, long poId,
            long poActivityId, AddActivityTimeSheetDto model)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.AccessDenied);

                var dbQuery = _poActivityRepository.Where(a =>
                    !a.PO.IsDeleted &&
                    a.POActivityId == poActivityId &&
                    a.POId == poId &&
                    !a.IsDeleted &&
                    a.PO.BaseContractCode == authenticate.ContractCode);


                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.EntityDoesNotExist);

                if (dbQuery.Any(a=>a.PO.POStatus==POStatus.Canceled))
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.CantDoneBecausePOCanceled);
                if (model.ProgressPercent<0)
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.ProgressPercentCantBelowZero);

                if (!dbQuery.Any(a => a.ActivityOwnerId == authenticate.UserId))
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.AccessDenied);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.AccessDenied);

                var poActivityModel = await dbQuery
                    .Include(a => a.ActivityTimesheets)
                    .Include(a => a.PO)
                    .ThenInclude(a => a.Supplier)
                    .FirstOrDefaultAsync();

                if (poActivityModel == null)
                    return ServiceResultFactory.CreateError<ResultAddActivityTimeSheetDto>(null, MessageId.EntityDoesNotExist);
                if (poActivityModel.ActivityTimesheets != null && poActivityModel.ActivityTimesheets.Any())
                {
                    if (poActivityModel.ActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) + model.ProgressPercent > 100)
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

                var activityTimeSheetModel = new POActivityTimesheet
                {
                    DateIssue = model.DateIssue.UnixTimestampToDateTime(),
                    Description = model.Description,
                    POActivityId = poActivityModel.POActivityId,
                    Duration =ts,
                    ProgressPercent=model.ProgressPercent
                };

                if (poActivityModel.ActivityTimesheets != null && poActivityModel.ActivityTimesheets.Any())
                    poActivityModel.ActivityTimesheets.Add(activityTimeSheetModel);
                else
                {
                    poActivityModel.ActivityTimesheets = new List<POActivityTimesheet>();
                    poActivityModel.ActivityTimesheets.Add(activityTimeSheetModel);
                }

                var sumDurationTimeSpan = new TimeSpan(poActivityModel.ActivityTimesheets.Where(a => !a.IsDeleted).Sum(v => v.Duration.Ticks));
                poActivityModel.Duration = sumDurationTimeSpan.TotalHours;

                var progressPercent = await UpdatePoProgress(poId, PoActivityType.AddActivityTimeSheet, poActivityModel);

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
                        },
                        ActivityProgressPercent =_activityTimesheetRepository.Where(a => !a.IsDeleted && a.POActivityId == poActivityModel.POActivityId).Sum(c => c.ProgressPercent),
                        PoProgressPercent = progressPercent,
                        ProgressPercent=activityTimeSheetModel.ProgressPercent
                        
                    };

                    List<int> activityUserIds = await GetPOActivityUserIdsAsync(poActivityModel.POId, poActivityModel.PO.AdderUserId.Value);

                    var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = poActivityModel.PO.Supplier.Name,
                        Message = poActivityModel.Description,
                        FormCode = poActivityModel.PO.POCode,
                        KeyValue = poActivityModel.POActivityId.ToString(),
                        Quantity = activityTimeSheetModel.Duration.ToString(@"hh\:mm"),
                        NotifEvent = NotifEvent.AddPOTimeSheet,
                        RootKeyValue = poActivityModel.POId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        ProductGroupId = poActivityModel.PO.ProductGroupId,
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

        public async Task<ServiceResult<List<ListActivityTimeSheetDto>>> GetActivityTimeSheetAsync(AuthenticateDto authenticate, long poId,
            long poActivityId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ListActivityTimeSheetDto>>(null, MessageId.AccessDenied);

                var dbQuery = _poActivityRepository
                    .AsNoTracking()
                    .Where(a => !a.PO.IsDeleted &&
                    a.POActivityId == poActivityId &&
                    a.POId == poId &&
                    !a.IsDeleted &&
                    a.PO.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<List<ListActivityTimeSheetDto>>(null, MessageId.EntityDoesNotExist);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<List<ListActivityTimeSheetDto>>(null, MessageId.AccessDenied);

                var result = await dbQuery.SelectMany(c => c.ActivityTimesheets.Where(v => !v.IsDeleted))
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

        public async Task<ServiceResult<ActivityTimeSheetDto>> DeleteActivityTimeSheetAsync(AuthenticateDto authenticate, long poId,
            long poActivityId, long activityTimeSheetId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<ActivityTimeSheetDto>(null, MessageId.AccessDenied);

                var dbQuery = _activityTimesheetRepository.Where(a =>
                    !a.IsDeleted &&
                    a.ActivityTimesheetId == activityTimeSheetId &&
                    !a.POActivity.PO.IsDeleted &&
                    a.POActivity.POActivityId == poActivityId &&
                    a.POActivity.POId == poId &&
                    !a.POActivity.IsDeleted &&
                    a.POActivity.PO.BaseContractCode == authenticate.ContractCode);

                if (dbQuery.Count() == 0)
                    return ServiceResultFactory.CreateError<ActivityTimeSheetDto>(null, MessageId.EntityDoesNotExist);

                if (!dbQuery.Any(a => a.POActivity.ActivityOwnerId == authenticate.UserId))
                    return ServiceResultFactory.CreateError<ActivityTimeSheetDto>(null, MessageId.AccessDenied);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.POActivity.PO.ProductGroupId)))
                    return ServiceResultFactory.CreateError<ActivityTimeSheetDto>(null, MessageId.AccessDenied);

                var activityTimeSheetModel = await dbQuery
                    .Include(a => a.POActivity)
                    .ThenInclude(a => a.PO)
                    .ThenInclude(a => a.Supplier)
                    .FirstOrDefaultAsync();

                if (activityTimeSheetModel == null)
                    return ServiceResultFactory.CreateError<ActivityTimeSheetDto>(null, MessageId.EntityDoesNotExist);

                activityTimeSheetModel.IsDeleted = true;

                var totalHour = activityTimeSheetModel.Duration.TotalHours;
                activityTimeSheetModel.POActivity.Duration -= totalHour;
                var progressPercent = await UpdatePoProgress(poId, PoActivityType.DeleteActivityTimeSheet, activityTimeSheetModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    List<int> activityUserIds = await GetPOActivityUserIdsAsync(activityTimeSheetModel.POActivity.POId,
                        activityTimeSheetModel.POActivity.PO.AdderUserId.Value);

                    var res1 = await _scmLogAndNotificationService.AddScmAuditLogAsync(new AddAuditLogDto
                    {
                        ContractCode = authenticate.ContractCode,
                        Description = activityTimeSheetModel.POActivity.PO.Supplier.Name,
                        Message = activityTimeSheetModel.Description,
                        FormCode = activityTimeSheetModel.POActivity.PO.POCode,
                        KeyValue = activityTimeSheetModel.POActivityId.ToString(),
                        Quantity = activityTimeSheetModel.Duration.ToString(@"hh\:mm"),
                        NotifEvent = NotifEvent.DeletePOTimeSheet,
                        RootKeyValue = activityTimeSheetModel.POActivity.POId.ToString(),
                        PerformerUserId = authenticate.UserId,
                        PerformerUserFullName = authenticate.UserFullName,
                        ProductGroupId = activityTimeSheetModel.POActivity.PO.ProductGroupId,
                        ReceiverLogUserIds = activityUserIds
                    }, null);
                    var result = new ActivityTimeSheetDto
                    {
                        TotalDuration = activityTimeSheetModel.POActivity.Duration.ToString(),
                        ActivityProgressPercent = _activityTimesheetRepository.Where(a => !a.IsDeleted && a.POActivityId == poActivityId).Sum(c => c.ProgressPercent),
                        PoProgressPercent = progressPercent,
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

        #endregion

        public async Task<ServiceResult<List<UserMentionDto>>> GetActivityUserListAsync(AuthenticateDto authenticate, long POId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateSuccess(new List<UserMentionDto>());

                var dbQuery = _poRepository
                    .Where(a => !a.IsDeleted && a.POId == POId && a.BaseContractCode == authenticate.ContractCode);

                var ProductGroupId = await dbQuery
                    .Select(c => c.ProductGroupId)
                    .FirstOrDefaultAsync();

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.ProductGroupId)))
                    return ServiceResultFactory.CreateSuccess(new List<UserMentionDto>());

                var roles = new List<string> { SCMRole.POActivityOwner, SCMRole.POActivityMng };
                var list = await _authenticationServices.GetAllUserHasAccessPOAsync(authenticate.ContractCode, roles, ProductGroupId);

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
        private async Task<double> UpdatePoProgress(long poId, PoActivityType type, object model)
        {
            double result = 0;
            var dbQuery = _poRepository
                 .Include(a => a.POProgresses)
                 .Where(a => !a.IsDeleted && a.POId == poId);
            if (dbQuery == null)
                return -1;
            else if (type == PoActivityType.CalculateProgress)
            {
                var po = await dbQuery.FirstOrDefaultAsync();
                return po.POProgresses.Where(a => !a.IsDeleted).OrderByDescending(a => a.Id).FirstOrDefault().ProgressPercent;
            }
            else
            {
                double average = 0;
                double weight = 0;
                var po = await dbQuery.FirstOrDefaultAsync();
                var poActivity = await _poActivityRepository.Include(a => a.ActivityTimesheets).Where(a => !a.IsDeleted && a.POId == poId).ToListAsync();
                if (type == PoActivityType.DeleteActivity)
                {
                    foreach (var item in poActivity.Where(a => a.POActivityId != (long)model))
                    {

                        if (item.ActivityTimesheets != null && item.ActivityTimesheets.Any())
                            average += item.ActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * item.Weight;
                        weight += item.Weight;
                    }
                }
                else if (type == PoActivityType.EditActivity)
                {
                    var poActivityModel = model as POActivity;
                    foreach (var item in poActivity.Where(a => a.POActivityId != poActivityModel.POActivityId))
                    {

                        if (item.ActivityTimesheets != null && item.ActivityTimesheets.Any())
                            average += item.ActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * item.Weight;
                        weight += item.Weight;
                    }
                    foreach (var item in poActivity.Where(a => a.POActivityId == poActivityModel.POActivityId))
                    {
                        if (item.ActivityTimesheets != null && item.ActivityTimesheets.Any())
                            average += item.ActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * poActivityModel.Weight;
                        weight += poActivityModel.Weight;
                    }
                }
                else if (type == PoActivityType.AddActivity)
                {
                    var poActivityModel = model as POActivity;
                    foreach (var item in poActivity)
                    {

                        if (item.ActivityTimesheets != null && item.ActivityTimesheets.Any())
                            average += item.ActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * item.Weight;
                        weight += item.Weight;
                    }

                    weight += poActivityModel.Weight;

                }
                else if (type == PoActivityType.SetPoActivityStatus)
                {
                    var poActivityModel = model as POActivity;
                    foreach (var item in poActivity.Where(a => a.POActivityId != poActivityModel.POActivityId))
                    {

                        if (item.ActivityTimesheets != null && item.ActivityTimesheets.Any())
                            average += item.ActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * item.Weight;
                        weight += item.Weight;
                    }

                    if (poActivityModel.ActivityTimesheets != null && poActivityModel.ActivityTimesheets.Any())
                        average += poActivityModel.ActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * poActivityModel.Weight;
                    weight += poActivityModel.Weight;

                }
                else if (type == PoActivityType.AddActivityTimeSheet)
                {
                    var poActivityModel = model as POActivity;
                    foreach (var item in poActivity.Where(a => a.POActivityId != poActivityModel.POActivityId))
                    {

                        if (item.ActivityTimesheets != null && item.ActivityTimesheets.Any())
                            average += item.ActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * item.Weight;
                        weight += item.Weight;
                    }

                    if (poActivityModel.ActivityTimesheets != null && poActivityModel.ActivityTimesheets.Any())
                        average += poActivityModel.ActivityTimesheets.Where(a => !a.IsDeleted).Sum(a => a.ProgressPercent) * poActivityModel.Weight;
                    weight += poActivityModel.Weight;
                }
                else if (type == PoActivityType.DeleteActivityTimeSheet)
                {
                    var poActivityTimesheetModel = model as POActivityTimesheet;
                    foreach (var item in poActivity)
                    {

                        if (item.ActivityTimesheets != null && item.ActivityTimesheets.Any())
                            average += item.ActivityTimesheets.Where(a => a.ActivityTimesheetId != poActivityTimesheetModel.ActivityTimesheetId && !a.IsDeleted).Sum(a => a.ProgressPercent) * item.Weight;
                        weight += item.Weight;
                    }



                }
                if (po.POProgresses != null && po.POProgresses.Any())
                {
                    if (poActivity != null && poActivity.Any())
                    {

                        po.POProgresses.Add(new PoProgress { IsDeleted = false, ProgressPercent = (weight > 0) ? average / weight : 0 });
                        result = (weight > 0) ? average / weight : 0;
                    }
                    else
                    {
                        po.POProgresses.Add(new PoProgress { IsDeleted = false, ProgressPercent = 0 });
                        result = 0;
                    }
                }

                else
                {
                    if (poActivity != null && poActivity.Any())
                    {
                        po.POProgresses = new List<PoProgress> { new PoProgress { IsDeleted = false, ProgressPercent = (weight > 0) ? average / weight : 0 } };
                        result = (weight > 0) ? average / weight : 0;
                    }

                    else
                    {
                        po.POProgresses = new List<PoProgress> { new PoProgress { IsDeleted = false, ProgressPercent = 0 } };
                        result = 0;
                    }
                }



                return result;
            }

        }

        public enum PoActivityType
        {
            DeleteActivity = 1,
            EditActivity = 2,
            SetPoActivityStatus = 3,
            AddActivityTimeSheet = 4,
            DeleteActivityTimeSheet = 5,
            AddActivity = 6,
            CalculateProgress = 7
        }
    }
}
