using Microsoft.EntityFrameworkCore;
using Raybod.SCM.DataAccess.Core;
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
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.Audit;
using Newtonsoft.Json;
using Raybod.SCM.DataAccess.Extention;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataTransferObject.Bom;
using Raybod.SCM.DataTransferObject.Product;
using Raybod.SCM.DataTransferObject.User;

namespace Raybod.SCM.Services.Implementation
{
    public class SCMLogAndNotificationService : ISCMLogAndNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly DbSet<SCMAuditLog> _scmAuditLogsRepository;
        private readonly DbSet<UserSeenScmAuditLog> _userSCMAuditLogsRepository;
        private readonly DbSet<Notification> _notificationRepository;
        private readonly DbSet<UserMentions> _mentionNotificationRepository;
        private readonly DbSet<UserNotification> _userNotificationRepository;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<UserNotify> _notifyRepository;
        private readonly DbSet<TeamWorkUser> _teamworkUserRepository;
        private readonly CompanyAppSettingsDto _appSettings;


        public SCMLogAndNotificationService(IUnitOfWork unitOfWork,
            ITeamWorkAuthenticationService authenticationService,
             IOptions<CompanyAppSettingsDto> appSettings)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _appSettings = appSettings.Value;
            _notificationRepository = _unitOfWork.Set<Notification>();
            _mentionNotificationRepository = _unitOfWork.Set<UserMentions>();
            _userRepository = _unitOfWork.Set<User>();
            _notifyRepository = _unitOfWork.Set<UserNotify>();
            _userNotificationRepository = _unitOfWork.Set<UserNotification>();
            _userSCMAuditLogsRepository = _unitOfWork.Set<UserSeenScmAuditLog>();
            _teamworkUserRepository = _unitOfWork.Set<TeamWorkUser>();
            _scmAuditLogsRepository = _unitOfWork.Set<SCMAuditLog>();
        }


        public async Task<ServiceResult<AuditLogNotificationDto>> GetAuditlogByUserIdAndPermission(AuthenticateDto authenticate, AuditLogQuery query)
        {
            try
            {
                var result = new AuditLogNotificationDto();
                var permission = await _authenticationService.GetUserLogPermissionEntitiesAsync(authenticate.UserId, authenticate.ContractCode);
                //if (permission.GlobalPermission.Count() == 0 && (user.UserType != (int)UserStatus.CustomerUser && user.UserType != (int)UserStatus.ConsultantUser))
                //    return ServiceResultFactory.CreateSuccess<AuditLogNotificationDto>(null);
                var dbQuery = _userSCMAuditLogsRepository
                    .AsNoTracking()
                    .Where(a => a.SCMAuditLog.BaseContractCode == authenticate.ContractCode && a.UserId == authenticate.UserId)
                    .AsQueryable();

                //dbQuery = dbQuery
                //    .Where(a => a.BaseContractCode == authenticate.ContractCode &&
                //   (((permission.GlobalPermission != null && permission.GlobalPermission.Contains(a.NotifEvent)) ||
                //   (a.UserPinAuditLogs != null && a.UserPinAuditLogs.Any(a => a.UserId == authenticate.UserId)) ||
                //    (a.LogUserReceivers != null && a.LogUserReceivers.Any(c => c.UserId == authenticate.UserId))) ||
                //    ((user.UserType == (int)UserStatus.CustomerUser || user.UserType == (int)UserStatus.ConsultantUser) &&
                //    (a.NotifEvent == NotifEvent.AddComComment || a.NotifEvent == NotifEvent.ReplyComComment || a.NotifEvent == NotifEvent.AddTransmittal))
                //    ));

                //if (permission.DocumentGroupIds.Any())
                //    dbQuery = dbQuery.Where(a => (a..DocumentGroupId == null || (a.LogUserReceivers != null && a.LogUserReceivers.Any(c => c.UserId == authenticate.UserId))) || (a.DocumentGroupId != null && permission.DocumentGroupIds.Contains(a.DocumentGroupId.Value)));

                //if (permission.ProductGroupIds.Any())
                //    dbQuery = dbQuery.Where(a => (a.ProductGroupId == null || (a.LogUserReceivers != null && a.LogUserReceivers.Any(c => c.UserId == authenticate.UserId))) || (a.ProductGroupId != null && permission.ProductGroupIds.Contains(a.ProductGroupId.Value)));

                //if (!string.IsNullOrEmpty(query.SearchText))
                //    dbQuery = dbQuery.Where(a =>
                //     (a.FormCode != null && a.FormCode.Contains(query.SearchText)) ||
                //     (a.Description != null && a.Description.Contains(query.SearchText)) ||
                //     (a.Quantity != null && a.Quantity.Contains(query.SearchText)) ||
                //     (a.Temp != null && a.Temp.Contains(query.SearchText)) ||
                //     (a.Message != null && a.Message.Contains(query.SearchText)));

                if (!string.IsNullOrEmpty(query.SearchText))
                {
                    var searchTerm = query.SearchText.Split("+");

                    foreach (string item in searchTerm)
                    {
                        string itemCopy = item.Trim();

                        dbQuery = dbQuery.Where(a => (a.SCMAuditLog.FormCode != null && a.SCMAuditLog.FormCode.Contains(itemCopy)) ||
                         (a.SCMAuditLog.Description != null && a.SCMAuditLog.Description.Contains(itemCopy)) ||
                         (a.SCMAuditLog.Quantity != null && a.SCMAuditLog.Quantity.Contains(itemCopy)) ||
                         (!String.IsNullOrEmpty(a.SCMAuditLog.PerformerUser.FullName) && a.SCMAuditLog.PerformerUser.FullName.Contains(itemCopy)) ||
                         (!String.IsNullOrEmpty(a.SCMAuditLog.PerformerUser.UserName) && a.SCMAuditLog.PerformerUser.UserName.Contains(itemCopy)) ||
                         (a.SCMAuditLog.Temp != null && a.SCMAuditLog.Temp.Contains(itemCopy)) ||
                         (a.SCMAuditLog.Message != null && a.SCMAuditLog.Message.Contains(itemCopy))

                        );


                    }
                }








                var totalCount = dbQuery.Count();
                result.UnSeenCount = dbQuery.Count(a => !a.IsSeen);
                dbQuery = dbQuery.OrderByDescending(a => a.PinDate).ThenByDescending(a => a.SCMAuditLog.DateCreate);
                dbQuery = dbQuery.ApplayPageing(query);

                result.Notifications = await dbQuery.Select(a => new AuditLogMiniInfoDto
                {
                    Id = a.SCMAuditLog.Id,
                    DateCreate = a.SCMAuditLog.DateCreate.ToUnixTimestamp(),
                    KeyValue = a.SCMAuditLog.KeyValue,
                    BaseContractCode = a.SCMAuditLog.BaseContractCode,
                    IsSeen = a.IsSeen,
                    Temp = a.SCMAuditLog.Temp,
                    RootKeyValue2 = a.SCMAuditLog.RootKeyValue2,
                    Description = a.SCMAuditLog.Description,
                    FormCode = a.SCMAuditLog.FormCode,
                    Quantity = a.SCMAuditLog.Quantity,
                    RootKeyValue = a.SCMAuditLog.RootKeyValue,
                    Message = a.SCMAuditLog.Message,
                    NotifEvent = a.SCMAuditLog.NotifEvent,
                    PerformerUserId = a.SCMAuditLog.PerformerUserId,
                    PerformerUserName = a.SCMAuditLog.PerformerUser.FullName,
                    EventNumber = !string.IsNullOrEmpty(a.SCMAuditLog.EventNumber) ? a.SCMAuditLog.EventNumber : "",
                    PerformerUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.SCMAuditLog.PerformerUser.Image,
                    IsPin = a.IsPin
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<AuditLogNotificationDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> SetSeenLogNotificationByUserIdAsync(AuthenticateDto authenticate)
        {
            try
            {
                var userPermission = await _authenticationService.GetUserLogPermissionEntitiesAsync(authenticate.UserId, authenticate.ContractCode);
                var user = await _userRepository.FindAsync(authenticate.UserId);
                if (userPermission == null && (user.UserType != (int)UserStatus.ConsultantUser && user.UserType != (int)UserStatus.CustomerUser))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (userPermission.GlobalPermission.Count() == 0 && (user.UserType != (int)UserStatus.ConsultantUser && user.UserType != (int)UserStatus.CustomerUser))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var auditLogs = await _userSCMAuditLogsRepository
                    .Where(a => a.SCMAuditLog.BaseContractCode == authenticate.ContractCode && a.UserId == authenticate.UserId && !a.IsSeen)
                    .ToListAsync();

                foreach (var item in auditLogs)
                {
                    item.IsSeen = true;
                }

                await _unitOfWork.SaveChangesAsync();

                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> UpdatePinEventByUserIdAsync(AuthenticateDto authenticate, Guid eventId)
        {
            try
            {
                var userPermission = await _authenticationService.GetUserLogPermissionEntitiesAsync(authenticate.UserId, authenticate.ContractCode);

                var auditLogs = await _userSCMAuditLogsRepository
                    .Where(a => a.UserId == authenticate.UserId && a.SCMAuditLog.BaseContractCode == authenticate.ContractCode &&
                    a.SCMAuditLogId == eventId)
                    .FirstOrDefaultAsync();

                if (auditLogs == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);


                if (auditLogs.IsPin)
                {
                    auditLogs.IsPin = false;
                    auditLogs.PinDate = null;
                }
                else
                {
                    auditLogs.IsPin = true;
                    auditLogs.PinDate = DateTime.Now;
                }



                await _unitOfWork.SaveChangesAsync();

                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> SetSeenMentionByUserIdAsync(AuthenticateDto authenticate)
        {
            try
            {
                var userPermission = await _authenticationService.GetUserLogPermissionEntitiesAsync(authenticate.UserId, authenticate.ContractCode);
                var user = await _userRepository.FindAsync(authenticate.UserId);
                if (userPermission == null && (user.UserType != (int)UserStatus.ConsultantUser && user.UserType != (int)UserStatus.CustomerUser))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (userPermission.GlobalPermission.Count() == 0 && (user.UserType != (int)UserStatus.ConsultantUser && user.UserType != (int)UserStatus.CustomerUser))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var mentions = await _mentionNotificationRepository
                    .Where(a => a.BaseContratcCode == authenticate.ContractCode && a.UserId == authenticate.UserId && !a.IsSeen).ToListAsync();



                foreach (var item in mentions)
                {
                    item.SeenDate = DateTime.Now;
                    item.IsSeen = true;
                }

                await _unitOfWork.SaveChangesAsync();

                return ServiceResultFactory.CreateSuccess(true);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<UserNotificationDto>> GetNotificationByUserIdAsync(AuthenticateDto authenticate, NotificationQueryDto query)
        {
            try
            {
                var result = new UserNotificationDto();
                List<NotifEvent> mentionNotif = new List<NotifEvent>
                {
                    NotifEvent.MentionComTeamComment,
                    NotifEvent.MentionOperationComment,
                    NotifEvent.MentionPOComment,
                    NotifEvent.MentionPOFinancialComment,
                    NotifEvent.MentionPOInspectionComment,
                    NotifEvent.MentionPOSupplierDocumentComment,
                    NotifEvent.AddCommercialRFPCommentMention,
                    NotifEvent.AddRFPProFormaCommentMention,
                    NotifEvent.AddTechRFPCommentMention,
                    NotifEvent.RevisionCommentUserMention
                };
                //var taskNotifies = await _notifyRepository.Where(a => a.TeamWork.ContractCode == authenticate.ContractCode && a.UserId == authenticate.UserId && a.IsActive && a.NotifyType == NotifyManagementType.Task).Select(a => a.NotifyNumber).ToListAsync();
                var dbQuery = _userNotificationRepository
                    .AsNoTracking()
                    .Where(a => a.Notification.BaseContratcCode == authenticate.ContractCode &&
                    a.UserId == authenticate.UserId && !mentionNotif.Contains(a.Notification.NotifEvent));

                //if (!string.IsNullOrEmpty(query.SearchText))
                //    dbQuery = dbQuery.Where(a =>
                //     (a.Notification.FormCode != null && a.Notification.FormCode.Contains(query.SearchText)) ||
                //     (a.Notification.Description != null && a.Notification.Description.Contains(query.SearchText)) ||
                //     (a.Notification.Quantity != null && a.Notification.Quantity.Contains(query.SearchText)) ||
                //     (a.Notification.Temp != null && a.Notification.Temp.Contains(query.SearchText)) ||
                //     (a.Notification.Message != null && a.Notification.Message.Contains(query.SearchText)));

                if (!string.IsNullOrEmpty(query.SearchText))
                {
                    var searchTerm = query.SearchText.Split("+");

                    foreach (string item in searchTerm)
                    {
                        string itemCopy = item.Trim();

                        dbQuery = dbQuery.Where(a => (a.Notification.FormCode != null && a.Notification.FormCode.Contains(itemCopy)) ||
                         (a.Notification.Description != null && a.Notification.Description.Contains(itemCopy)) ||
                         (a.Notification.Quantity != null && a.Notification.Quantity.Contains(itemCopy)) ||
                         ((!String.IsNullOrEmpty(a.Notification.PerformerUser.FullName) && a.Notification.PerformerUser.FullName.Contains(itemCopy))) ||
                         ((!String.IsNullOrEmpty(a.Notification.PerformerUser.UserName) && a.Notification.PerformerUser.UserName.Contains(itemCopy))) ||
                         (a.Notification.Temp != null && a.Notification.Temp.Contains(itemCopy)) ||
                         (a.Notification.Message != null && a.Notification.Message.Contains(itemCopy))

                        );
                    }
                }







                var totalCount = dbQuery.Count();
                result.UnDoneCount = dbQuery.Count(a => !a.Notification.IsDone && !a.IsSeen);

                dbQuery = dbQuery
                    .OrderByDescending(a => a.PinDate)
                    .ThenBy(a => a.IsSeen)
                    .ThenBy(a => a.Notification.IsDone)
                    .ThenByDescending(a => a.Notification.DateCreate)
                    .ApplayPageing(query.Page, query.PageSize);

                result.Notifications = await dbQuery
                    .Select(c => new BaseNotificationDto
                    {
                        Id = c.Notification.Id,
                        BaseContractCode = c.Notification.BaseContratcCode,
                        NotifEvent = c.Notification.NotifEvent,
                        FormCode = c.Notification.FormCode,
                        Message = c.Notification.Message,
                        Description = c.Notification.Description,
                        Quantity = c.Notification.Quantity,
                        Temp = c.Notification.Temp,
                        KeyValue = c.Notification.KeyValue,
                        RootKeyValue = c.Notification.RootKeyValue,
                        RootKeyValue2 = c.Notification.RootKeyValue2,
                        DateCreate = c.Notification.DateCreate.ToUnixTimestamp(),
                        IsDone = c.IsSeen ? true : c.Notification.IsDone,
                        UserId = authenticate.UserId,
                        IsPin = c.IsPin,
                        PerformerUserId = c.Notification.PerformerUserId,
                        PerformerUserName = c.Notification.PerformerUser.FullName,
                        PerformerUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.Notification.PerformerUser.Image
                    }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<UserNotificationDto>(null, exception);
            }
        }
        public async Task<ServiceResult<MentionNotificationDto>> GetMentionNotificationByUserIdAsync(AuthenticateDto authenticate, NotificationQueryDto query)
        {
            try
            {
                var result = new MentionNotificationDto();
                var dbQuery = _mentionNotificationRepository
                    .AsNoTracking()
                    .Where(a => a.BaseContratcCode == authenticate.ContractCode &&
                    a.UserId == authenticate.UserId);


                if (!string.IsNullOrEmpty(query.SearchText))
                {
                    var searchTerm = query.SearchText.Split("+");

                    foreach (string item in searchTerm)
                    {
                        string itemCopy = item.Trim();
                        dbQuery = dbQuery.Where(a => (a.FormCode != null && a.FormCode.Contains(itemCopy)) ||
                         (a.Description != null && a.Description.Contains(itemCopy)) ||
                         ((!String.IsNullOrEmpty(a.PerformerUser.FullName) && a.PerformerUser.FullName.Contains(itemCopy))) ||
                         ((!String.IsNullOrEmpty(a.PerformerUser.UserName) && a.PerformerUser.UserName.Contains(itemCopy))) ||
                         (a.Message != null && a.Message.Contains(itemCopy)));
                    }
                }







                var totalCount = dbQuery.Count();
                result.UnDoneCount = dbQuery.Count(a => !a.IsSeen);

                dbQuery = dbQuery
                    .OrderByDescending(a => a.PinDate)
                    .ThenBy(a => a.IsSeen)
                    .ThenByDescending(a => a.DateCreate)
                    .ApplayPageing(query.Page, query.PageSize);


                result.Notifications = await dbQuery
                    .Select(c => new BaseMentionNotificationDto
                    {
                        Id = c.Id,
                        BaseContractCode = c.BaseContratcCode,
                        MentionEvent = c.MentionEvent,
                        FormCode = c.FormCode,
                        Message = c.Message,
                        Description = c.Description,
                        KeyValue = c.KeyValue,
                        RootKeyValue = c.RootKeyValue,
                        RootKeyValue2 = c.RootKeyValue2,
                        DateCreate = c.DateCreate.ToUnixTimestamp(),
                        IsSeen = c.IsSeen,
                        IsPin = c.IsPin,
                        UserId = authenticate.UserId,
                        Temp = c.Temp,
                        PerformerUserId = c.PerformerUserId,
                        PerformerUserName = c.PerformerUser.FullName,
                        PerformerUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + c.PerformerUser.Image
                    }).ToListAsync();
                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<MentionNotificationDto>(null, exception);
            }
        }
        public async Task<ServiceResult<List<AllUserContractTaskBadge>>> GetAllContractNotificationBadgeByUserIdAsync(AuthenticateDto authenticate)
        {
            try
            {
                var userTasks = await _notificationRepository
                    .AsNoTracking()
                    .Where(a => a.BaseContratcCode == authenticate.ContractCode &&
                    a.IsDone &&
                    a.UserNotifications.Any(v => v.UserId == authenticate.UserId))
                    .Select(a => new
                    {
                        contractCode = a.BaseContratcCode
                    }).ToListAsync();

                var result = userTasks.GroupBy(a => a.contractCode)
                    .Select(c => new AllUserContractTaskBadge
                    {
                        ContractCode = c.Key,
                        Count = c.Count()
                    }).ToList();

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<AllUserContractTaskBadge>(), exception);
            }
        }
        public async Task<ServiceResult<UserEventsBadgeDto>> GetAllContractEventsBadgeByUserIdAsync(AuthenticateDto authenticate)
        {
            try
            {
                var taskUnSeen = await GetNotificationByUserIdAsync(authenticate, new NotificationQueryDto());

                var eventUnSeen = await GetAuditlogByUserIdAndPermission(authenticate, new AuditLogQuery());



                var mentionUnSeen = await _mentionNotificationRepository.AsNoTracking().Where(a => a.BaseContratcCode == authenticate.ContractCode && a.UserId == authenticate.UserId && !a.IsSeen).ToListAsync();
                UserEventsBadgeDto result = new UserEventsBadgeDto();
                result.UnSeenEventCount = eventUnSeen.Result.UnSeenCount;
                result.UnSeenMentionCount = mentionUnSeen.Count;
                result.UnSeenNotificationCount = taskUnSeen.Result.UnDoneCount;
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<UserEventsBadgeDto>(null, exception);
            }
        }
        public async Task<ServiceResult<bool>> SetSeenUserNotificationAsync(AuthenticateDto authenticate, Guid notificationId)
        {
            try
            {
                var notificationModel = await _notificationRepository.Where(a => a.Id == notificationId &&
                 a.UserNotifications.Any(c => c.UserId == authenticate.UserId))
                    .Include(a => a.UserNotifications)
                    .FirstOrDefaultAsync();

                if (notificationModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var mentions = new List<NotifEvent> {
                    NotifEvent.AddTechRFPCommentMention, // 30
                    NotifEvent.AddCommercialRFPCommentMention,//31
                    NotifEvent.MentionPOComment, // 119
                    NotifEvent.RevisionCommentUserMention, // 105
                    NotifEvent.MentionComTeamComment // 117
                    };

                if (mentions.Contains(notificationModel.NotifEvent))
                {
                    notificationModel.IsDone = true;
                    notificationModel.DateDone = DateTime.UtcNow;
                }

                var notifUser = notificationModel.UserNotifications.FirstOrDefault(a => a.UserId == authenticate.UserId);

                notifUser.IsSeen = true;
                notifUser.DateSeen = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> UpdatePinNotificationAsync(AuthenticateDto authenticate, Guid notificationId)
        {
            try
            {
                var notification = await _notificationRepository.Include(a => a.UserNotifications).Where(a => a.Id == notificationId &&
                   a.UserNotifications.Any(a => a.UserId == authenticate.UserId)).FirstOrDefaultAsync();

                if (notification == null || notification.UserNotifications == null || !notification.UserNotifications.Any())
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                var userNotification = notification.UserNotifications.FirstOrDefault(a => a.UserId == authenticate.UserId);
                if (userNotification == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (userNotification.IsPin)
                {
                    userNotification.IsPin = false;
                    userNotification.PinDate = null;
                }
                else
                {
                    userNotification.IsPin = true;
                    userNotification.PinDate = DateTime.UtcNow;
                }

                await _unitOfWork.SaveChangesAsync();

                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> SetSeenUserNotificationByUserIdAsync(AuthenticateDto authenticate)
        {
            try
            {
                var notificationModel = await _userNotificationRepository.Where(a => a.UserId == authenticate.UserId && a.Notification.BaseContratcCode == authenticate.ContractCode && !a.IsSeen).ToListAsync();

                if (notificationModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                foreach (var item in notificationModel)
                {


                    item.IsSeen = true;
                    item.DateSeen = DateTime.UtcNow;
                }


                await _unitOfWork.SaveChangesAsync();

                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> SetSeenMentionNotificationAsync(AuthenticateDto authenticate, Guid mentionId)
        {
            try
            {
                var mentionModel = await _mentionNotificationRepository.Where(a => a.Id == mentionId &&
                 a.UserId == authenticate.UserId).FirstOrDefaultAsync();

                if (mentionModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);


                mentionModel.IsSeen = true;
                mentionModel.SeenDate = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> UpdatePinMentionNotificationAsync(AuthenticateDto authenticate, Guid mentionId)
        {
            try
            {
                var mentionModel = await _mentionNotificationRepository.Where(a => a.Id == mentionId &&
                 a.UserId == authenticate.UserId).FirstOrDefaultAsync();

                if (mentionModel == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (mentionModel.IsPin)
                {
                    mentionModel.IsPin = false;
                    mentionModel.PinDate = null;
                }
                else
                {
                    mentionModel.IsPin = true;
                    mentionModel.PinDate = DateTime.UtcNow;
                }

                await _unitOfWork.SaveChangesAsync();

                return ServiceResultFactory.CreateSuccess(true);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        #region scm log service
        public async Task<ServiceResult<bool>> AddScmAuditLogAsync(AddAuditLogDto AddModel, List<NotifToDto> notifTo)
        {
            try
            {
                var scmAuditLogModel = new SCMAuditLog
                {
                    BaseContractCode = AddModel.ContractCode,
                    DateCreate = DateTime.UtcNow,
                    KeyValue = AddModel.KeyValue,
                    Message = AddModel.Message,
                    NotifEvent = AddModel.NotifEvent,
                    Temp = AddModel.Temp,
                    RootKeyValue2 = AddModel.RootKeyValue2,
                    Quantity = AddModel.Quantity,
                    FormCode = AddModel.FormCode,
                    Description = AddModel.Description,
                    RootKeyValue = AddModel.RootKeyValue,
                    PerformerUserId = AddModel.PerformerUserId,
                    DocumentGroupId = AddModel.DocumentGroupId,
                    ProductGroupId = AddModel.ProductGroupId,
                    EventNumber = (await _scmAuditLogsRepository.Where(a => a.BaseContractCode == AddModel.ContractCode).CountAsync() + 1).ToString(),
                    UserSCMAuditLogs = new List<UserSeenScmAuditLog>()
                };
                scmAuditLogModel = await AddUserReceiver(scmAuditLogModel, AddModel.ContractCode, AddModel.NotifEvent);
                if (AddModel.ReceiverLogUserIds.Any())
                {
                    scmAuditLogModel.LogUserReceivers = AddModel.ReceiverLogUserIds.Select(userId => new LogUserReceiver
                    {
                        UserId = userId
                    }).ToList();
                }

                if (notifTo != null)
                {
                    var notifRecipientUsers = await GetNotifReceipientUsersAsync(AddModel.ContractCode, notifTo);
                    if (notifRecipientUsers != null || notifRecipientUsers.Count() > 0)
                    {
                        foreach (var item in notifRecipientUsers)
                        {
                            var sendScmNotifUsers = item.UserNotifConfigs.Select(a => a.UserId).ToList();
                            var sendNotification = SendNotificationByLog(AddModel, sendScmNotifUsers, item.NotifEvent);
                            if (sendNotification != null)
                                _notificationRepository.AddRange(sendNotification);
                        }
                    }
                }

                _scmAuditLogsRepository.Add(scmAuditLogModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {

                    return ServiceResultFactory.CreateSuccess(true);

                }
                return ServiceResultFactory.CreateError(false, MessageId.NotificationStateNotSaved);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddMentionTaskAsync(AddAuditLogDto AddModel)
        {
            try
            {
                foreach (var item in AddModel.ReceiverLogUserIds)
                {
                    var notificationModel = new Notification
                    {
                        BaseContratcCode = AddModel.ContractCode,
                        DateCreate = DateTime.UtcNow,
                        KeyValue = AddModel.KeyValue,
                        Message = AddModel.Message,
                        NotifEvent = AddModel.NotifEvent,
                        Temp = AddModel.Temp,
                        RootKeyValue2 = AddModel.RootKeyValue2,
                        Quantity = AddModel.Quantity,
                        FormCode = AddModel.FormCode,
                        Description = AddModel.Description,
                        RootKeyValue = AddModel.RootKeyValue,
                        PerformerUserId = AddModel.PerformerUserId,
                        UserNotifications = new List<UserNotification> { new UserNotification
                        {
                            UserId = item
                        }},

                    };
                    _notificationRepository.Add(notificationModel);
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {



                    return ServiceResultFactory.CreateSuccess(true);

                }
                return ServiceResultFactory.CreateError(false, MessageId.NotificationStateNotSaved);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddMentionNotificationTaskAsync(AddMentionLogDto AddModel)
        {
            try
            {
                foreach (var item in AddModel.ReceiverLogUserIds)
                {
                    var notificationModel = new UserMentions
                    {
                        BaseContratcCode = AddModel.ContractCode,
                        DateCreate = DateTime.UtcNow,
                        KeyValue = AddModel.KeyValue,
                        Message = AddModel.Message,
                        MentionEvent = AddModel.MentionEvent,
                        RootKeyValue2 = AddModel.RootKeyValue2,
                        FormCode = AddModel.FormCode,
                        Description = AddModel.Description,
                        RootKeyValue = AddModel.RootKeyValue,
                        PerformerUserId = AddModel.PerformerUserId,
                        Temp = AddModel.Temp,
                        UserId = item,
                        IsSeen = false,
                        IsPin = false
                    };
                    _mentionNotificationRepository.Add(notificationModel);
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);

                }
                return ServiceResultFactory.CreateError(false, MessageId.NotificationStateNotSaved);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> AddScmAuditLogAsync(AddAuditLogDto AddModel, int productGroupId, List<NotifToDto> notifTo)
        {
            try
            {
                var scmAuditLogModel = new SCMAuditLog
                {
                    BaseContractCode = AddModel.ContractCode,
                    DateCreate = DateTime.UtcNow,
                    KeyValue = AddModel.KeyValue,
                    Message = AddModel.Message,
                    NotifEvent = AddModel.NotifEvent,
                    Temp = AddModel.Temp,
                    RootKeyValue2 = AddModel.RootKeyValue2,
                    Quantity = AddModel.Quantity,
                    FormCode = AddModel.FormCode,
                    Description = AddModel.Description,
                    RootKeyValue = AddModel.RootKeyValue,
                    PerformerUserId = AddModel.PerformerUserId,
                    DocumentGroupId = AddModel.DocumentGroupId,
                    ProductGroupId = AddModel.ProductGroupId,
                    EventNumber = (await _scmAuditLogsRepository.Where(a => a.BaseContractCode == AddModel.ContractCode).CountAsync() + 1).ToString(),
                    UserSCMAuditLogs = new List<UserSeenScmAuditLog>()

                };
                scmAuditLogModel = await AddUserReceiver(scmAuditLogModel, AddModel.ContractCode, AddModel.NotifEvent);
                if (AddModel.ReceiverLogUserIds.Any())
                {
                    scmAuditLogModel.LogUserReceivers = AddModel.ReceiverLogUserIds.Select(userId => new LogUserReceiver
                    {
                        UserId = userId
                    }).ToList();
                }

                if (notifTo != null && notifTo.Any())
                {
                    var notifRecipientUsers = await GetNotifReceipientUsersAsync(AddModel.ContractCode, notifTo, null, productGroupId);
                    if (notifRecipientUsers != null || notifRecipientUsers.Count() > 0)
                    {
                        foreach (var item in notifRecipientUsers)
                        {
                            var sendScmNotifUsers = item.UserNotifConfigs.Select(a => a.UserId).ToList();
                            var sendNotification = SendNotificationByLog(AddModel, sendScmNotifUsers, item.NotifEvent);
                            if (sendNotification != null)
                                _notificationRepository.AddRange(sendNotification);
                        }
                    }
                }

                _scmAuditLogsRepository.Add(scmAuditLogModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);

                }
                return ServiceResultFactory.CreateError(false, MessageId.NotificationStateNotSaved);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> AddScmAuditLogAsync(AddAuditLogDto AddModel, int productGroupId, NotifEvent notifEvent, int? userId)
        {
            try
            {
                var scmAuditLogModel = new SCMAuditLog
                {
                    BaseContractCode = AddModel.ContractCode,
                    DateCreate = DateTime.UtcNow,
                    KeyValue = AddModel.KeyValue,
                    Message = AddModel.Message,
                    NotifEvent = AddModel.NotifEvent,
                    Temp = AddModel.Temp,
                    RootKeyValue2 = AddModel.RootKeyValue2,
                    Quantity = AddModel.Quantity,
                    FormCode = AddModel.FormCode,
                    Description = AddModel.Description,
                    RootKeyValue = AddModel.RootKeyValue,
                    PerformerUserId = AddModel.PerformerUserId,
                    DocumentGroupId = AddModel.DocumentGroupId,
                    ProductGroupId = AddModel.ProductGroupId,
                    EventNumber = (await _scmAuditLogsRepository.Where(a => a.BaseContractCode == AddModel.ContractCode).CountAsync() + 1).ToString(),
                    UserSCMAuditLogs = new List<UserSeenScmAuditLog>()
                };
                scmAuditLogModel = await AddUserReceiver(scmAuditLogModel, AddModel.ContractCode, AddModel.NotifEvent);
                if (AddModel.ReceiverLogUserIds.Any())
                {
                    scmAuditLogModel.LogUserReceivers = AddModel.ReceiverLogUserIds.Select(userId => new LogUserReceiver
                    {
                        UserId = userId
                    }).ToList();
                }

                if (userId != null)
                {
                    var sendNotification = SendNotificationByLog(AddModel, new List<int> { userId.Value }, notifEvent);
                    if (sendNotification != null)
                        _notificationRepository.Add(sendNotification);
                }



                _scmAuditLogsRepository.Add(scmAuditLogModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);

                }
                return ServiceResultFactory.CreateError(false, MessageId.NotificationStateNotSaved);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> AddDocumentAuditLogAsync(AddAuditLogDto AddModel, int documentGroupId, List<NotifToDto> notifTo)
        {
            try
            {
                var scmAuditLogModel = new SCMAuditLog
                {
                    BaseContractCode = AddModel.ContractCode,
                    DateCreate = DateTime.UtcNow,
                    KeyValue = AddModel.KeyValue,
                    Message = AddModel.Message,
                    NotifEvent = AddModel.NotifEvent,
                    Temp = AddModel.Temp,
                    RootKeyValue2 = AddModel.RootKeyValue2,
                    Quantity = AddModel.Quantity,
                    FormCode = AddModel.FormCode,
                    Description = AddModel.Description,
                    RootKeyValue = AddModel.RootKeyValue,
                    PerformerUserId = AddModel.PerformerUserId,
                    ProductGroupId = AddModel.ProductGroupId,
                    DocumentGroupId = AddModel.DocumentGroupId,
                    EventNumber = (await _scmAuditLogsRepository.Where(a => a.BaseContractCode == AddModel.ContractCode).CountAsync() + 1).ToString(),
                    UserSCMAuditLogs = new List<UserSeenScmAuditLog>()

                };
                scmAuditLogModel = await AddUserReceiver(scmAuditLogModel, AddModel.ContractCode, AddModel.NotifEvent);
                if (AddModel.ReceiverLogUserIds.Any())
                {
                    scmAuditLogModel.LogUserReceivers = AddModel.ReceiverLogUserIds.Select(userId => new LogUserReceiver
                    {
                        UserId = userId
                    }).ToList();
                }

                if (notifTo != null)
                {
                    var notifRecipientUsers = await GetNotifReceipientUsersAsync(AddModel.ContractCode, notifTo, documentGroupId, null);
                    if (notifRecipientUsers != null || notifRecipientUsers.Count() > 0)
                    {
                        foreach (var item in notifRecipientUsers)
                        {
                            var sendScmNotifUsers = item.UserNotifConfigs.Select(a => a.UserId).ToList();
                            var sendNotification = SendNotificationByLog(AddModel, sendScmNotifUsers, item.NotifEvent);
                            if (sendNotification != null)
                                _notificationRepository.AddRange(sendNotification);
                        }
                    }
                }

                _scmAuditLogsRepository.Add(scmAuditLogModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);

                }
                return ServiceResultFactory.CreateError(false, MessageId.NotificationStateNotSaved);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddOperationAuditLogAsync(AddAuditLogDto AddModel, int OperaitonGroupId, List<NotifToDto> notifTo)
        {
            try
            {
                var scmAuditLogModel = new SCMAuditLog
                {
                    BaseContractCode = AddModel.ContractCode,
                    DateCreate = DateTime.UtcNow,
                    KeyValue = AddModel.KeyValue,
                    Message = AddModel.Message,
                    NotifEvent = AddModel.NotifEvent,
                    Temp = AddModel.Temp,
                    RootKeyValue2 = AddModel.RootKeyValue2,
                    Quantity = AddModel.Quantity,
                    FormCode = AddModel.FormCode,
                    Description = AddModel.Description,
                    RootKeyValue = AddModel.RootKeyValue,
                    PerformerUserId = AddModel.PerformerUserId,
                    ProductGroupId = AddModel.ProductGroupId,
                    DocumentGroupId = AddModel.DocumentGroupId,
                    OperationGroupId = AddModel.OperationGroupId,
                    EventNumber = (await _scmAuditLogsRepository.Where(a => a.BaseContractCode == AddModel.ContractCode).CountAsync() + 1).ToString(),
                    UserSCMAuditLogs = new List<UserSeenScmAuditLog>()

                };
                scmAuditLogModel = await AddUserReceiver(scmAuditLogModel, AddModel.ContractCode, AddModel.NotifEvent);
                if (AddModel.ReceiverLogUserIds.Any())
                {
                    scmAuditLogModel.LogUserReceivers = AddModel.ReceiverLogUserIds.Select(userId => new LogUserReceiver
                    {
                        UserId = userId
                    }).ToList();
                }

                if (notifTo != null)
                {
                    var notifRecipientUsers = await GetNotifReceipientUsersAsync(AddModel.ContractCode, notifTo, null, null, OperaitonGroupId);
                    if (notifRecipientUsers != null || notifRecipientUsers.Count() > 0)
                    {
                        foreach (var item in notifRecipientUsers)
                        {
                            var sendScmNotifUsers = item.UserNotifConfigs.Select(a => a.UserId).ToList();
                            var sendNotification = SendNotificationByLog(AddModel, sendScmNotifUsers, item.NotifEvent);
                            if (sendNotification != null)
                                _notificationRepository.AddRange(sendNotification);
                        }
                    }
                }

                _scmAuditLogsRepository.Add(scmAuditLogModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);

                }
                return ServiceResultFactory.CreateError(false, MessageId.NotificationStateNotSaved);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddScmAuditLogAndTaskAsync(AddAuditLogDto auditLogModel, AddTaskNotificationDto taskModel)
        {
            try
            {
                var scmAuditLogModel = new SCMAuditLog
                {
                    BaseContractCode = auditLogModel.ContractCode,
                    FormCode = auditLogModel.FormCode,
                    Description = auditLogModel.Description,
                    Message = auditLogModel.Message,
                    NotifEvent = auditLogModel.NotifEvent,
                    Quantity = auditLogModel.Quantity,
                    KeyValue = auditLogModel.KeyValue,
                    RootKeyValue = auditLogModel.RootKeyValue,
                    DateCreate = DateTime.UtcNow,
                    PerformerUserId = auditLogModel.PerformerUserId,
                    Temp = auditLogModel.Temp,
                    RootKeyValue2 = auditLogModel.RootKeyValue2,
                    DocumentGroupId = auditLogModel.DocumentGroupId,
                    ProductGroupId = auditLogModel.ProductGroupId,
                    EventNumber = (await _scmAuditLogsRepository.Where(a => a.BaseContractCode == auditLogModel.ContractCode).CountAsync() + 1).ToString(),
                    UserSCMAuditLogs = new List<UserSeenScmAuditLog>()

                };
                scmAuditLogModel = await AddUserReceiver(scmAuditLogModel, auditLogModel.ContractCode, auditLogModel.NotifEvent);
                if (auditLogModel.ReceiverLogUserIds.Any())
                {
                    scmAuditLogModel.LogUserReceivers = auditLogModel.ReceiverLogUserIds.Select(userId => new LogUserReceiver
                    {
                        UserId = userId
                    }).ToList();
                }

                if (taskModel != null)
                {
                    var notifModel = new Notification
                    {
                        DateCreate = DateTime.UtcNow,
                        NotifEvent = taskModel.NotifEvent,
                        KeyValue = taskModel.KeyValue,
                        BaseContratcCode = taskModel.ContractCode,
                        Temp = taskModel.Temp,
                        RootKeyValue2 = taskModel.RootKeyValue2,
                        Quantity = taskModel.Quantity,
                        FormCode = taskModel.FormCode,
                        Description = taskModel.Description,
                        RootKeyValue = taskModel.RootKeyValue,
                        Message = taskModel.Message,
                        PerformerUserId = taskModel.PerformerUserId,
                        UserNotifications = taskModel.Users.Select(userId => new UserNotification
                        {
                            UserId = userId,
                        }).ToList()
                    };

                    _notificationRepository.Add(notifModel);
                }

                _scmAuditLogsRepository.Add(scmAuditLogModel);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);

                }
                return ServiceResultFactory.CreateError(false, MessageId.NotificationStateNotSaved);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }




        public async Task<ServiceResult<bool>> AddPendingPOTaskNotificationAsync(AddAuditLogDto AddModel, NotifToDto notifTo,
          string contractCode, List<long> poIds)
        {
            try
            {
                var accessUsers = await _authenticationService.GetTeamWorkRolesByRolesAndContractCode(contractCode, notifTo.Roles, null, AddModel.ProductGroupId);
                var userIds = accessUsers.Select(a => a.UserId).Distinct().ToList();
                var taskNotifyUsers = await _notifyRepository.Where(a => a.IsActive && a.TeamWork.ContractCode == contractCode && a.NotifyType == NotifyManagementType.Task && (int)notifTo.NotifEvent == (int)a.NotifyNumber).Select(a => a.UserId).ToListAsync();
                if (userIds != null && userIds.Any() && taskNotifyUsers != null && taskNotifyUsers.Any())
                {
                    userIds = userIds.Where(a => taskNotifyUsers.Contains(a)).ToList();
                }
                else
                    userIds = new List<int>();
                if (userIds != null && userIds.Any())
                {
                    foreach (var poId in poIds)
                    {
                        var notifModels = new Notification
                        {
                            DateCreate = DateTime.UtcNow,
                            NotifEvent = notifTo.NotifEvent,
                            KeyValue = poId.ToString(),
                            BaseContratcCode = contractCode,
                            Temp = AddModel.Temp,
                            RootKeyValue2 = AddModel.RootKeyValue2,
                            Quantity = AddModel.Quantity,
                            FormCode = AddModel.FormCode,
                            Description = AddModel.Description,
                            RootKeyValue = AddModel.RootKeyValue,
                            Message = AddModel.Message,
                            PerformerUserId = AddModel.PerformerUserId,
                            UserNotifications = userIds.Select(c => new UserNotification
                            {
                                UserId = c
                            }).ToList()
                        };
                        _notificationRepository.Add(notifModels);
                    }
                }

                return await _unitOfWork.SaveChangesAsync() > 0
                     ? ServiceResultFactory.CreateSuccess(true)
                     : ServiceResultFactory.CreateError(false, MessageId.NotificationStateNotSaved);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddScmAuditLogAsync(List<AddAuditLogDto> AddModels, List<string> sendNotifRoles)
        {
            try
            {
                int index = 1;
                var scmAuditLogModel = new List<SCMAuditLog>();
                foreach (var log in AddModels)
                {
                    var logModel = new SCMAuditLog
                    {
                        BaseContractCode = log.ContractCode,
                        DateCreate = DateTime.UtcNow,
                        KeyValue = log.KeyValue,
                        Message = log.Message,
                        NotifEvent = log.NotifEvent,
                        PerformerUserId = log.PerformerUserId,
                        Temp = log.Temp,
                        RootKeyValue2 = log.RootKeyValue2,
                        Description = log.Description,
                        FormCode = log.FormCode,
                        Quantity = log.Quantity,
                        RootKeyValue = log.RootKeyValue,
                        ProductGroupId = log.ProductGroupId,
                        DocumentGroupId = log.DocumentGroupId,
                        EventNumber = (await _scmAuditLogsRepository.Where(a => a.BaseContractCode == log.ContractCode).CountAsync() + index).ToString(),
                        UserSCMAuditLogs = new List<UserSeenScmAuditLog>()
                    };
                    logModel = await AddUserReceiver(logModel, log.ContractCode, log.NotifEvent);
                    index++;
                    if (log.ReceiverLogUserIds.Any())
                    {
                        logModel.LogUserReceivers = log.ReceiverLogUserIds.Select(userId => new LogUserReceiver
                        {
                            UserId = userId
                        }).ToList();
                    }
                    scmAuditLogModel.Add(logModel);
                }

                if (sendNotifRoles != null && sendNotifRoles.Count() > 0)
                {
                    var notifRecipientUsers = await GetNotifReceipientUsersAsync(AddModels.First(), sendNotifRoles);
                    if (notifRecipientUsers != null || notifRecipientUsers.Count() > 0)
                    {
                        var sendScmNotifUsers = notifRecipientUsers.Select(a => a.UserId).ToList();
                        var sendNotification = POPreprationSendNotificationByLog(AddModels.First(), sendScmNotifUsers);
                        if (sendNotification != null)
                            _notificationRepository.Add(sendNotification);
                    }
                }

                _scmAuditLogsRepository.AddRange(scmAuditLogModel);
                return await _unitOfWork.SaveChangesAsync() > 0
                     ? ServiceResultFactory.CreateSuccess(true)
                     : ServiceResultFactory.CreateError(false, MessageId.NotificationStateNotSaved);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddScmAuditLogAsync(AddAuditLogDto AddModel, NotifEvent notifEvent, int? userId)
        {
            try
            {
                var scmAuditLogModel = new List<SCMAuditLog>();

                var logModel = new SCMAuditLog
                {
                    BaseContractCode = AddModel.ContractCode,
                    DateCreate = DateTime.UtcNow,
                    KeyValue = AddModel.KeyValue,
                    Message = AddModel.Message,
                    NotifEvent = AddModel.NotifEvent,
                    PerformerUserId = AddModel.PerformerUserId,
                    Temp = AddModel.Temp,
                    RootKeyValue2 = AddModel.RootKeyValue2,
                    Description = AddModel.Description,
                    FormCode = AddModel.FormCode,
                    Quantity = AddModel.Quantity,
                    RootKeyValue = AddModel.RootKeyValue,
                    ProductGroupId = AddModel.ProductGroupId,
                    DocumentGroupId = AddModel.DocumentGroupId,
                    EventNumber = (await _scmAuditLogsRepository.Where(a => a.BaseContractCode == AddModel.ContractCode).CountAsync() + 1).ToString(),
                    UserSCMAuditLogs = new List<UserSeenScmAuditLog>()

                };
                logModel = await AddUserReceiver(logModel, AddModel.ContractCode, AddModel.NotifEvent);
                if (AddModel.ReceiverLogUserIds.Any())
                {
                    logModel.LogUserReceivers = AddModel.ReceiverLogUserIds.Select(userId => new LogUserReceiver
                    {
                        UserId = userId
                    }).ToList();
                }
                scmAuditLogModel.Add(logModel);


                if (userId != null)
                {
                    var sendNotification = SendNotificationByLog(AddModel, new List<int> { userId.Value }, notifEvent);
                    if (sendNotification != null)
                        _notificationRepository.Add(sendNotification);
                }

                _scmAuditLogsRepository.AddRange(scmAuditLogModel);
                return await _unitOfWork.SaveChangesAsync() > 0
                     ? ServiceResultFactory.CreateSuccess(true)
                     : ServiceResultFactory.CreateError(false, MessageId.NotificationStateNotSaved);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> AddScmAuditLogAsync(List<AddAuditLogDto> AddModels, List<AddTaskNotificationDto> notifTo, bool notif)
        {
            try
            {
                int index = 1;
                var scmAuditLogModel = new List<SCMAuditLog>();
                foreach (var log in AddModels)
                {
                    var logModel = new SCMAuditLog
                    {
                        BaseContractCode = log.ContractCode,
                        DateCreate = DateTime.UtcNow,
                        KeyValue = log.KeyValue,
                        Message = log.Message,
                        NotifEvent = log.NotifEvent,
                        PerformerUserId = log.PerformerUserId,
                        Temp = log.Temp,
                        RootKeyValue2 = log.RootKeyValue2,
                        Description = log.Description,
                        FormCode = log.FormCode,
                        Quantity = log.Quantity,
                        RootKeyValue = log.RootKeyValue,
                        ProductGroupId = log.ProductGroupId,
                        DocumentGroupId = log.DocumentGroupId,
                        EventNumber = (await _scmAuditLogsRepository.Where(a => a.BaseContractCode == log.ContractCode).CountAsync() + index).ToString(),
                        UserSCMAuditLogs = new List<UserSeenScmAuditLog>()
                    };
                    logModel = await AddUserReceiver(logModel, log.ContractCode, log.NotifEvent);
                    index++;
                    if (log.ReceiverLogUserIds.Any())
                    {
                        logModel.LogUserReceivers = log.ReceiverLogUserIds.Select(userId => new LogUserReceiver
                        {
                            UserId = userId
                        }).ToList();
                    }
                    scmAuditLogModel.Add(logModel);
                }

                if (notifTo != null && notifTo.Any())
                {
                    var notifModels = new List<Notification>();
                    foreach (var item in notifTo)
                    {
                        var notifModel = new Notification
                        {
                            DateCreate = DateTime.UtcNow,
                            NotifEvent = item.NotifEvent,
                            KeyValue = item.KeyValue,
                            BaseContratcCode = item.ContractCode,
                            Temp = item.Temp,
                            RootKeyValue2 = item.RootKeyValue2,
                            Quantity = item.Quantity,
                            FormCode = item.FormCode,
                            Description = item.Description,
                            RootKeyValue = item.RootKeyValue,
                            Message = item.Message,
                            PerformerUserId = item.PerformerUserId,
                            UserNotifications = item.Users.Select(userId => new UserNotification
                            {
                                UserId = userId,
                            }).ToList()
                        };
                        notifModels.Add(notifModel);
                    }


                    _notificationRepository.AddRange(notifModels);
                }

                _scmAuditLogsRepository.AddRange(scmAuditLogModel);
                return await _unitOfWork.SaveChangesAsync() > 0
                     ? ServiceResultFactory.CreateSuccess(true)
                     : ServiceResultFactory.CreateError(false, MessageId.NotificationStateNotSaved);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> AddDocumentNotificationOnCreateContract(AuthenticateDto authenticate, string contractCode,
            List<DocumentGroup> documentGroups, NotifEvent notifEvent, List<string> roles)
        {
            var documentGroupIds = documentGroups.Select(a => a.DocumentGroupId).ToList();
            var userModels = await _authenticationService.GetTeamWorkRolesByRolesAndContractCode(contractCode, documentGroupIds, roles);

            if (userModels == null || !userModels.Any())
                return ServiceResultFactory.CreateSuccess(true);
            foreach (var item in documentGroups)
            {
                var users = userModels
                    .Where(v => !v.TeamWorkUserDocumentGroups.Any() || v.TeamWorkUserDocumentGroups.Any(c => c.DocumentGroupId == item.DocumentGroupId))
                    .Select(c => c.UserId)
                    .Distinct()
                    .ToList();

                if (users == null || !users.Any())
                    continue;

                var notifModel = new Notification
                {

                    DateCreate = DateTime.UtcNow,
                    NotifEvent = notifEvent,
                    BaseContratcCode = contractCode,
                    Description = item.Title,
                    FormCode = item.DocumentGroupCode,
                    RootKeyValue = item.DocumentGroupId.ToString(),
                    KeyValue = item.DocumentGroupId.ToString(),
                    Message = "",
                    PerformerUserId = authenticate.UserId,
                    UserNotifications = users.Select(userId => new UserNotification
                    {
                        UserId = userId
                    }).ToList()
                };

                _notificationRepository.Add(notifModel);
            }

            return await _unitOfWork.SaveChangesAsync() > 0
                ? ServiceResultFactory.CreateSuccess(true)
                : ServiceResultFactory.CreateError(false, MessageId.AddEntityFailed);
        }

        #endregion

        #region scm notification service

        private async Task<List<UserNotifConfigDto>> GetNotifReceipientUsersAsync(AddAuditLogDto log, List<string> rendNotifRoles)
        {
            try
            {
                var users = await _authenticationService.GetTeamWorkRolesByRolesAndContractCode(log.ContractCode, rendNotifRoles);
                if (users == null || !users.Any())
                    return null;
                var taskNotifyUsers = await _notifyRepository.Where(a => a.IsActive && a.TeamWork.ContractCode == log.ContractCode && a.NotifyType == NotifyManagementType.Task && (int)log.NotifEvent == (int)a.NotifyNumber).Select(a => a.UserId).ToListAsync();
                if (taskNotifyUsers != null && taskNotifyUsers.Any())
                    users = users.Where(a => taskNotifyUsers.Contains(a.UserId)).ToList();
                else
                    users = new List<TeamWorkUserRole>();
                if (users == null || !users.Any())
                    return null;

                return users.GroupBy(a => a.UserId)
                     .Select(tr => new UserNotifConfigDto
                     {
                         UserId = tr.Key,
                     }).ToList();
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        private async Task<List<NotifeRecipientUserDto>> GetNotifReceipientUsersAsync(string contractCode, List<NotifToDto> rendNotifRoles)
        {
            try
            {
                var result = new List<NotifeRecipientUserDto>();

                foreach (var item in rendNotifRoles)
                {

                    var users = await _authenticationService.GetTeamWorkRolesByRolesAndContractCode(contractCode, item.Roles);
                    if (users == null || !users.Any())
                        return null;
                    var taskNotifyUsers = await _notifyRepository.Where(a => a.IsActive && a.TeamWork.ContractCode == contractCode && a.NotifyType == NotifyManagementType.Task && (int)item.NotifEvent == (int)a.NotifyNumber).Select(a => a.UserId).ToListAsync();
                    if (taskNotifyUsers != null && taskNotifyUsers.Any())
                        users = users.Where(a => taskNotifyUsers.Contains(a.UserId)).ToList();
                    else
                        users = new List<TeamWorkUserRole>();
                    if (users == null || !users.Any())
                        return null;
                    var notifeRecipientUsers = users.GroupBy(a => a.UserId)
                        .Select(tr => new UserNotifConfigDto
                        {
                            UserId = tr.Key,
                        }).ToList();

                    if (notifeRecipientUsers != null && notifeRecipientUsers.Count() > 0)
                        result.Add(new NotifeRecipientUserDto
                        {
                            NotifEvent = item.NotifEvent,
                            UserNotifConfigs = notifeRecipientUsers
                        });
                }
                return result;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        private async Task<List<NotifeRecipientUserDto>> GetNotifReceipientUsersAsync(string contractCode, List<NotifToDto> rendNotifRoles, int? documentGroupId, int? productGroupId)
        {
            try
            {
                var result = new List<NotifeRecipientUserDto>();

                foreach (var item in rendNotifRoles)
                {

                    var users = await _authenticationService.GetTeamWorkRolesByRolesAndContractCode(contractCode, item.Roles, documentGroupId, productGroupId);
                    if (users == null || !users.Any())
                        return null;
                    var taskNotifyUsers = await _notifyRepository.Where(a => a.IsActive && a.TeamWork.ContractCode == contractCode && a.NotifyType == NotifyManagementType.Task && (int)item.NotifEvent == (int)a.NotifyNumber).Select(a => a.UserId).ToListAsync();
                    if (taskNotifyUsers != null && taskNotifyUsers.Any())
                        users = users.Where(a => taskNotifyUsers.Contains(a.UserId)).ToList();
                    else
                        users = new List<TeamWorkUserRole>();
                    if (users == null || !users.Any())
                        return null;

                    var notifeRecipientUsers = users.GroupBy(a => a.UserId)
                        .Select(tr => new UserNotifConfigDto
                        {
                            UserId = tr.Key,
                        }).ToList();

                    if (notifeRecipientUsers != null && notifeRecipientUsers.Count() > 0)
                        result.Add(new NotifeRecipientUserDto
                        {
                            NotifEvent = item.NotifEvent,
                            UserNotifConfigs = notifeRecipientUsers
                        });
                }
                return result;
            }
            catch (Exception exception)
            {
                return null;
            }
        }
        private async Task<List<NotifeRecipientUserDto>> GetNotifReceipientUsersAsync(string contractCode, List<NotifToDto> rendNotifRoles, int? documentGroupId, int? productGroupId, int? operationGroup)
        {
            try
            {
                var result = new List<NotifeRecipientUserDto>();

                foreach (var item in rendNotifRoles)
                {

                    var users = await _authenticationService.GetTeamWorkRolesByRolesAndContractCode(contractCode, item.Roles, documentGroupId, productGroupId, operationGroup);
                    if (users == null || !users.Any())
                        return null;
                    var taskNotifyUsers = await _notifyRepository.Where(a => a.IsActive && a.TeamWork.ContractCode == contractCode && a.NotifyType == NotifyManagementType.Task && (int)item.NotifEvent == (int)a.NotifyNumber).Select(a => a.UserId).ToListAsync();
                    if (taskNotifyUsers != null && taskNotifyUsers.Any())
                        users = users.Where(a => taskNotifyUsers.Contains(a.UserId)).ToList();
                    else
                        users = new List<TeamWorkUserRole>();
                    if (users == null || !users.Any())
                        return null;

                    var notifeRecipientUsers = users.GroupBy(a => a.UserId)
                        .Select(tr => new UserNotifConfigDto
                        {
                            UserId = tr.Key,
                        }).ToList();

                    if (notifeRecipientUsers != null && notifeRecipientUsers.Count() > 0)
                        result.Add(new NotifeRecipientUserDto
                        {
                            NotifEvent = item.NotifEvent,
                            UserNotifConfigs = notifeRecipientUsers
                        });
                }
                return result;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        private async Task<List<NotifeRecipientUserDto>> GetNotifReceipientUsersAsync(List<string> contractCodes,
            List<ListBomInfoDto> ProductGroups, List<NotifToDto> rendNotifRoles)
        {
            try
            {
                var result = new List<NotifeRecipientUserDto>();

                foreach (var item in rendNotifRoles)
                {

                    var users = await _authenticationService.GetTeamWorkRolesByRolesAndContractCode(contractCodes, item.Roles);
                    if (users == null || !users.Any())
                        return null;
                    var taskNotifyUsers = await _notifyRepository.Where(a => a.IsActive && contractCodes.Contains(a.TeamWork.ContractCode) && a.NotifyType == NotifyManagementType.Task && (int)item.NotifEvent == (int)a.NotifyNumber).Select(a => a.UserId).ToListAsync();
                    if (taskNotifyUsers != null && taskNotifyUsers.Any())
                        users = users.Where(a => taskNotifyUsers.Contains(a.UserId)).ToList();
                    else
                        users = new List<TeamWorkUserRole>();
                    if (users == null || !users.Any())
                        return null;

                    var globalUsers = users.Where(a => a.ContractCode == null).ToList();
                    var globalUserIds = globalUsers.GroupBy(a => a.UserId).Distinct().ToList();
                    var notifeRecipientUsers = users.Except(globalUsers).GroupBy(a => new { a.ContractCode, a.UserId }).ToList();
                    var usergetNotifList = new List<UserNotifConfigDto>();

                    foreach (var user in globalUserIds)
                    {
                        foreach (var code in contractCodes)
                        {
                            usergetNotifList.Add(new UserNotifConfigDto
                            {
                                UserId = user.Key,
                                ContractCode = code
                            });
                        }
                    }

                    foreach (var user in notifeRecipientUsers)
                    {
                        if (usergetNotifList == null || !usergetNotifList.Any())
                        {
                            usergetNotifList.Add(new UserNotifConfigDto
                            {
                                UserId = user.Key.UserId,
                                ContractCode = user.Key.ContractCode
                            });
                        }
                        else
                        {
                            if (!usergetNotifList.Any(c => c.UserId == user.Key.UserId && c.ContractCode == user.Key.ContractCode))
                            {
                                usergetNotifList.Add(new UserNotifConfigDto
                                {
                                    UserId = user.Key.UserId,
                                    ContractCode = user.Key.ContractCode
                                });
                            }
                        }
                    }

                    if (usergetNotifList != null && usergetNotifList.Count() > 0)
                        result.Add(new NotifeRecipientUserDto
                        {
                            NotifEvent = item.NotifEvent,
                            UserNotifConfigs = usergetNotifList
                        });
                }
                return result;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        private Notification POPreprationSendNotificationByLog(AddAuditLogDto AddAuditLogModel, List<int> userIds)
        {
            try
            {

                if (userIds == null || userIds.Count() == 0)
                    return null;

                var notifModel = new Notification
                {

                    DateCreate = DateTime.UtcNow,
                    NotifEvent = AddAuditLogModel.NotifEvent,
                    Temp = AddAuditLogModel.Temp,
                    RootKeyValue2 = AddAuditLogModel.RootKeyValue2,
                    Description = AddAuditLogModel.Description,
                    FormCode = AddAuditLogModel.FormCode,
                    Quantity = AddAuditLogModel.Quantity,
                    RootKeyValue = AddAuditLogModel.RootKeyValue,
                    KeyValue = AddAuditLogModel.KeyValue,
                    Message = GetPOPreparationEventMessage(AddAuditLogModel),
                    PerformerUserId = AddAuditLogModel.PerformerUserId,
                    UserNotifications = userIds.Select(userId => new UserNotification
                    {
                        UserId = userId
                    }).ToList()
                };

                return notifModel;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        private Notification SendNotificationByLog(AddAuditLogDto scmAuditLogModel, List<int> userIds, NotifEvent notifEvent)
        {
            try
            {

                if (userIds == null || userIds.Count() == 0)
                    return null;

                var notifModel = new Notification
                {
                    DateCreate = DateTime.UtcNow,
                    NotifEvent = notifEvent,
                    KeyValue = scmAuditLogModel.KeyValue,
                    BaseContratcCode = scmAuditLogModel.ContractCode,
                    Temp = scmAuditLogModel.Temp,
                    RootKeyValue2 = scmAuditLogModel.RootKeyValue2,
                    Quantity = scmAuditLogModel.Quantity,
                    FormCode = scmAuditLogModel.FormCode,
                    Description = scmAuditLogModel.Description,
                    RootKeyValue = scmAuditLogModel.RootKeyValue,
                    Message = scmAuditLogModel.Message,
                    PerformerUserId = scmAuditLogModel.PerformerUserId,
                    UserNotifications = userIds.Select(userId => new UserNotification
                    {
                        UserId = userId,
                    }).ToList()
                };

                return notifModel;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<ServiceResult<bool>> SetDonedNotificationAsync(int userId, string contractCode, string keyValue, NotifEvent notifEvent)
        {
            try
            {
                var notificationModels = await _notificationRepository
                    .Where(a => !a.IsDone && a.BaseContratcCode == contractCode && a.KeyValue == keyValue && a.NotifEvent == notifEvent)
                    .Include(a => a.UserNotifications)
                    .ToListAsync();

                foreach (var item in notificationModels)
                {

                    item.IsDone = true;
                    item.DateDone = DateTime.UtcNow;

                    if (item.UserNotifications != null && item.UserNotifications.Any(c => c.UserId == userId))
                    {
                        var model = item.UserNotifications.FirstOrDefault(c => c.UserId == userId);
                        model.IsUserSetTaskDone = true;
                    }
                }

                return await _unitOfWork.SaveChangesAsync() > 0
                     ? ServiceResultFactory.CreateSuccess(true)
                     : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> SetDonedNotificationAsync(int userId, string contractCode, string keyValue, string quantity, NotifEvent notifEvent)
        {
            try
            {
                var notificationModels = await _notificationRepository
                    .Where(a => !a.IsDone && a.NotifEvent == notifEvent && a.BaseContratcCode == contractCode && a.KeyValue == keyValue && a.Quantity == quantity)
                    .Include(a => a.UserNotifications)
                    .ToListAsync();

                foreach (var item in notificationModels)
                {

                    item.IsDone = true;
                    item.DateDone = DateTime.UtcNow;

                    if (item.UserNotifications != null && item.UserNotifications.Any(c => c.UserId == userId))
                    {
                        var model = item.UserNotifications.FirstOrDefault(c => c.UserId == userId);
                        model.IsUserSetTaskDone = true;
                    }
                }
                return await _unitOfWork.SaveChangesAsync() > 0
                     ? ServiceResultFactory.CreateSuccess(true)
                     : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> SetDonedNotificationByRootKeyValueAsync(int userId, string contractCode, string rootKeyValue, NotifEvent notifEvent)
        {
            try
            {
                var notificationModels = await _notificationRepository
                    .Where(a => !a.IsDone &&
                    a.BaseContratcCode == contractCode &&
                    a.RootKeyValue == rootKeyValue &&
                    a.NotifEvent == notifEvent)
                    .Include(a => a.UserNotifications)
                    .ToListAsync();

                foreach (var item in notificationModels)
                {

                    item.IsDone = true;
                    item.DateDone = DateTime.UtcNow;

                    if (item.UserNotifications != null && item.UserNotifications.Any(c => c.UserId == userId))
                    {
                        var model = item.UserNotifications.FirstOrDefault(c => c.UserId == userId);
                        model.IsUserSetTaskDone = true;
                    }
                }

                return await _unitOfWork.SaveChangesAsync() > 0
                     ? ServiceResultFactory.CreateSuccess(true)
                     : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> SetDonedNotificationAsync(int userId, string contractCode, List<string> keyValues, NotifEvent notifEvent)
        {
            try
            {
                var notificationModels = await _notificationRepository
                    .Where(a => !a.IsDone && a.BaseContratcCode == contractCode && keyValues.Contains(a.KeyValue) && a.NotifEvent == notifEvent)
                    .Include(a => a.UserNotifications)
                    .ToListAsync();

                if (notificationModels.Any())
                    notificationModels = notificationModels.Where(c => keyValues.Any(f => f == c.KeyValue)).ToList();

                foreach (var item in notificationModels)
                {
                    item.IsDone = true;
                    item.DateDone = DateTime.UtcNow;

                    if (item.UserNotifications != null && item.UserNotifications.Any(v => v.UserId == userId))
                    {
                        var model = item.UserNotifications.FirstOrDefault(v => v.UserId == userId);
                        model.IsUserSetTaskDone = true;
                    }
                }

                return await _unitOfWork.SaveChangesAsync() > 0
                     ? ServiceResultFactory.CreateSuccess(true)
                     : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> SetDonedNotificationAsync(int userId, List<string> contractCodes, NotifEvent notifEvent)
        {
            try
            {
                var notificationModels = await _notificationRepository
                    .Where(a => !a.IsDone && contractCodes.Contains(a.BaseContratcCode) && a.NotifEvent == notifEvent)
                    .Include(a => a.UserNotifications)
                    .ToListAsync();

                foreach (var item in notificationModels)
                {
                    item.IsDone = true;
                    item.DateDone = DateTime.UtcNow;

                    if (item.UserNotifications != null && item.UserNotifications.Any(v => v.UserId == userId))
                    {
                        var model = item.UserNotifications.FirstOrDefault(v => v.UserId == userId);
                        model.IsUserSetTaskDone = true;
                    }
                }

                return await _unitOfWork.SaveChangesAsync() > 0
                     ? ServiceResultFactory.CreateSuccess(true)
                     : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> SetDonedNotificationAsync(int userId, string contractCode, NotifEvent notifEvent)
        {
            try
            {
                var notificationModels = await _notificationRepository
                    .Where(a => !a.IsDone && a.BaseContratcCode == contractCode && a.NotifEvent == notifEvent)
                     .Include(a => a.UserNotifications)
                    .FirstOrDefaultAsync();

                notificationModels.IsDone = true;
                notificationModels.DateDone = DateTime.UtcNow;

                if (notificationModels.UserNotifications != null && notificationModels.UserNotifications.Any(c => c.UserId == userId))
                {
                    var model = notificationModels.UserNotifications.FirstOrDefault(c => c.UserId == userId);
                    model.IsUserSetTaskDone = true;
                }

                return await _unitOfWork.SaveChangesAsync() > 0
                     ? ServiceResultFactory.CreateSuccess(true)
                     : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> SetDonedNotificationAsync(string contractCode, string formCode, string keyValue, NotifEvent notifEvent)
        {
            try
            {
                var notificationModels = await _notificationRepository
                    .Where(a => !a.IsDone && a.BaseContratcCode == contractCode && a.NotifEvent == notifEvent && a.KeyValue == keyValue && a.FormCode == formCode)
                     .Include(a => a.UserNotifications)
                    .FirstOrDefaultAsync();
                if (notificationModels != null)
                {
                    notificationModels.IsDone = true;
                    notificationModels.DateDone = DateTime.UtcNow;
                    if (notificationModels.UserNotifications != null)
                    {
                        var model = notificationModels.UserNotifications.FirstOrDefault();
                        model.IsUserSetTaskDone = true;
                    }
                }
                    

                

                return await _unitOfWork.SaveChangesAsync() > 0
                     ? ServiceResultFactory.CreateSuccess(true)
                     : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }



        public async Task<ServiceResult<bool>> RemoveNotificationAsync(string contractCode, string keyValue, NotifEvent notifEvent)
        {
            try
            {
                var notificationModels = await _notificationRepository
                    .Where(a => !a.IsDone && a.BaseContratcCode == contractCode && a.KeyValue == keyValue && a.NotifEvent == notifEvent)
                    .FirstOrDefaultAsync();

                _notificationRepository.Remove(notificationModels);

                return await _unitOfWork.SaveChangesAsync() > 0
                 ? ServiceResultFactory.CreateSuccess(true)
                 : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }


        public string SerializerObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, JsonConfig());
        }

        #endregion

        private JsonSerializerSettings JsonConfig()
        {
            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return jss;
        }

        private string GetPOPreparationEventMessage(AddAuditLogDto addModel)
        {
            switch (addModel.NotifEvent)
            {
                //case NotifEvent.AddPOEngineeringActivity:
                //    return $" فعالیت  {addModel.Description} " +
                //        $" در آماده سازی سفارش - مهندسی توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} ثبت گردید.";

                //case NotifEvent.AddPOConstructionActivity:
                //    return $" فعالیت  {addModel.Description} " +
                //        $" در آماده سازی سفارش - ساخت توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} ثبت گردید.";

                //case NotifEvent.AddPOOtherActivity:
                //    return $" فعالیت  {addModel.Description} " +
                //        $" در آماده سازی سفارش - دیگر خدمات توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} ثبت گردید.";

                //case NotifEvent.AddPOPurchasingActivity:
                //    return $" فعالیت  {addModel.Description} " +
                //        $" در آماده سازی سفارش - خرید توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} ثبت گردید.";

                //case NotifEvent.AddPOConstructionActivitySubmitProgress:
                //    return $" اعلام پیشرفت فعالیت  {addModel.Description} " +
                //        $" از {addModel.BeforeChangeValue} به {addModel.AfterChangeValue} در آماده سازی سفارش - ساخت توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} تغییر یافت.";

                //case NotifEvent.AddPOEngineeringActivitySubmitProgress:
                //    return $" اعلام پیشرفت فعالیت  {addModel.Description} " +
                //        $" از {addModel.BeforeChangeValue} به {addModel.AfterChangeValue} در آماده سازی سفارش - مهندسی توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} تغییر یافت.";

                //case NotifEvent.AddPOOtherActivitySubmitProgress:
                //    return $" اعلام پیشرفت فعالیت  {addModel.Description} " +
                //        $" از {addModel.BeforeChangeValue} به {addModel.AfterChangeValue} در آماده سازی سفارش - دیگر خدمات توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} تغییر یافت.";

                //case NotifEvent.AddPOPurchasingActivitySubmitProgress:
                //    return $" اعلام پیشرفت فعالیت  {addModel.Description} " +
                //        $" از {addModel.BeforeChangeValue} به {addModel.AfterChangeValue} در آماده سازی سفارش - خرید توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} تغییر یافت.";

                //case NotifEvent.ConfirmPOConstructionActivityProgress:
                //    return $" تایید پیشرفت فعالیت  {addModel.Description} " +
                //            $" از {addModel.BeforeChangeValue} به {addModel.AfterChangeValue} در آماده سازی سفارش - ساخت توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} تغییر یافت.";

                //case NotifEvent.ConfirmPOEngineeringActivityProgress:
                //    return $" تایید پیشرفت فعالیت  {addModel.Description} " +
                //            $" از {addModel.BeforeChangeValue} به {addModel.AfterChangeValue} در آماده سازی سفارش - مهندسی توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} تغییر یافت.";

                //case NotifEvent.ConfirmPOOtherActivityProgress:
                //    return $" تایید پیشرفت فعالیت  {addModel.Description} " +
                //            $" از {addModel.BeforeChangeValue} به {addModel.AfterChangeValue} در آماده سازی سفارش - دیگر خدمات توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} تغییر یافت.";

                //case NotifEvent.ConfirmPOPurchasingActivityProgress:
                //    return $" تایید پیشرفت فعالیت  {addModel.Description} " +
                //            $" از {addModel.BeforeChangeValue} به {addModel.AfterChangeValue} در آماده سازی سفارش - خرید توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} تغییر یافت.";

                //case NotifEvent.EditPOConstructionActivity:
                //    return $" اطلاعات اولیه  {addModel.Description} " +
                //       $" در آماده سازی سفارش - ساخت توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} ثبت گردید.";

                //case NotifEvent.EditPOEngineeringActivity:
                //    return $" اطلاعات اولیه  {addModel.Description} " +
                //       $" در آماده سازی سفارش - مهندسی توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} ثبت گردید.";

                //case NotifEvent.EditPOOtherActivity:
                //    return $" اطلاعات اولیه  {addModel.Description} " +
                //       $" در آماده سازی سفارش - دیگر خدمات توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} ثبت گردید.";

                //case NotifEvent.EditPOPurchasingActivity:
                //    return $" اطلاعات اولیه  {addModel.Description} " +
                //       $" در آماده سازی سفارش - خرید توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} ثبت گردید.";

                //case NotifEvent.AddPOConstructionActivityQC:
                //    return $" رویداد کنترل کیفیت جدید برای فعالیت  {addModel.Description} " +
                //      $" در آماده سازی سفارش - ساخت توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} ثبت گردید.";

                //case NotifEvent.AddPOEngineeringActivityQC:
                //    return $" رویداد کنترل کیفیت جدید برای فعالیت  {addModel.Description} " +
                //      $" در آماده سازی سفارش - مهندسی توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} ثبت گردید.";

                //case NotifEvent.AddPOOtherActivityQC:
                //    return $" رویداد کنترل کیفیت جدید برای فعالیت  {addModel.Description} " +
                //      $" در آماده سازی سفارش - دیگر خدمات توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} ثبت گردید.";

                //case NotifEvent.AddPOPurchasingActivityQC:
                //    return $" رویداد کنترل کیفیت جدید برای فعالیت  {addModel.Description} " +
                //      $" در آماده سازی سفارش - خرید توسط کاربر {addModel.PerformerUserFullName} در تاریخ {DateTime.UtcNow.ToPersianDate()} ثبت گردید.";

                default:
                    return "یه خبری شده";
            }
        }
        private async Task<SCMAuditLog> AddUserReceiver(SCMAuditLog sCMAuditLog, string contractCode, NotifEvent notifEvent)
        {
            var users = await _authenticationService.GetSCMEventLogReceiverUserByNotifEventAndContractCode(contractCode, notifEvent);
            var eventNotifyUsers = await _notifyRepository.Where(a => a.IsActive && a.TeamWork.ContractCode == contractCode && a.NotifyType == NotifyManagementType.Event && (int)notifEvent == (int)a.NotifyNumber).Select(a => a.UserId).ToListAsync();
            if (eventNotifyUsers != null && eventNotifyUsers.Any())
                users = users.Where(a => eventNotifyUsers.Contains(a.UserId)).ToList();
            else
                users = new List<UserInfoForAuditLogDto>();
            foreach (var item in users)
            {
                if (sCMAuditLog.DocumentGroupId != null && (item.DocumentGroupIds == null || !item.DocumentGroupIds.Any() || item.DocumentGroupIds.Contains(sCMAuditLog.DocumentGroupId.Value)))
                {
                    if (!sCMAuditLog.UserSCMAuditLogs.Any(a => a.UserId == item.UserId))
                        sCMAuditLog.UserSCMAuditLogs.Add(new UserSeenScmAuditLog
                        {
                            IsPin = false,
                            IsSeen = false,
                            UserId = item.UserId
                        });
                }

                else if (sCMAuditLog.ProductGroupId != null && (item.ProductGroupIds == null || !item.ProductGroupIds.Any() || item.ProductGroupIds.Contains(sCMAuditLog.ProductGroupId.Value)))
                {
                    if (!sCMAuditLog.UserSCMAuditLogs.Any(a => a.UserId == item.UserId))
                        sCMAuditLog.UserSCMAuditLogs.Add(new UserSeenScmAuditLog
                        {
                            IsPin = false,
                            IsSeen = false,
                            UserId = item.UserId
                        });
                }
                else if (sCMAuditLog.OperationGroupId != null && (item.OperationGroupIds == null || !item.OperationGroupIds.Any() || item.OperationGroupIds.Contains(sCMAuditLog.OperationGroupId.Value)))
                {
                    if (!sCMAuditLog.UserSCMAuditLogs.Any(a => a.UserId == item.UserId))
                        sCMAuditLog.UserSCMAuditLogs.Add(new UserSeenScmAuditLog
                        {
                            IsPin = false,
                            IsSeen = false,
                            UserId = item.UserId
                        });
                }
                else if (sCMAuditLog.OperationGroupId == null && sCMAuditLog.DocumentGroupId == null && sCMAuditLog.ProductGroupId == null)
                {
                    if (!sCMAuditLog.UserSCMAuditLogs.Any(a => a.UserId == item.UserId))
                        sCMAuditLog.UserSCMAuditLogs.Add(new UserSeenScmAuditLog
                        {
                            IsPin = false,
                            IsSeen = false,
                            UserId = item.UserId
                        });
                }
            }
            if (notifEvent == NotifEvent.AddComComment || notifEvent == NotifEvent.ReplyComComment || notifEvent == NotifEvent.AddTransmittal)
            {
                var customerUsers = await _teamworkUserRepository.Where(a => a.TeamWork.ContractCode == contractCode && (a.User.UserType == (int)UserStatus.CustomerUser || a.User.UserType == (int)UserStatus.ConsultantUser)).ToListAsync();
                if (eventNotifyUsers != null && eventNotifyUsers.Any() && customerUsers != null && customerUsers.Any())
                    customerUsers = customerUsers.Where(a => eventNotifyUsers.Contains(a.UserId)).ToList();
                foreach (var item in customerUsers)
                {
                    if (!sCMAuditLog.UserSCMAuditLogs.Any(a => a.UserId == item.UserId))
                        sCMAuditLog.UserSCMAuditLogs.Add(new UserSeenScmAuditLog
                        {
                            IsPin = false,
                            IsSeen = false,
                            UserId = item.UserId
                        });
                }
            }
            return sCMAuditLog;
        }
    }
}
