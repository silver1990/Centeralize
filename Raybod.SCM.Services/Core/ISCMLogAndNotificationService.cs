using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Bom;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.DataTransferObject.Product;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface ISCMLogAndNotificationService
    {
        Task<ServiceResult<bool>> AddMentionTaskAsync(AddAuditLogDto AddModel);
        Task<ServiceResult<bool>> AddMentionNotificationTaskAsync(AddMentionLogDto AddModel);
        Task<ServiceResult<AuditLogNotificationDto>> GetAuditlogByUserIdAndPermission(AuthenticateDto authenticate, AuditLogQuery query);

        Task<ServiceResult<bool>> SetSeenLogNotificationByUserIdAsync(AuthenticateDto authenticate);
        Task<ServiceResult<bool>> UpdatePinEventByUserIdAsync(AuthenticateDto authenticate, Guid eventId);
        Task<ServiceResult<bool>> SetSeenMentionByUserIdAsync(AuthenticateDto authenticate);
        Task<ServiceResult<bool>> SetSeenUserNotificationAsync(AuthenticateDto authenticate, Guid notificationId);
        Task<ServiceResult<bool>> UpdatePinNotificationAsync(AuthenticateDto authenticate, Guid notificationId);
        Task<ServiceResult<bool>> SetSeenUserNotificationByUserIdAsync(AuthenticateDto authenticate);
        Task<ServiceResult<bool>> SetSeenMentionNotificationAsync(AuthenticateDto authenticate, Guid mentionId);
        Task<ServiceResult<bool>> UpdatePinMentionNotificationAsync(AuthenticateDto authenticate, Guid mentionId);
        Task<ServiceResult<UserNotificationDto>> GetNotificationByUserIdAsync(AuthenticateDto authenticate, NotificationQueryDto query);
        Task<ServiceResult<MentionNotificationDto>> GetMentionNotificationByUserIdAsync(AuthenticateDto authenticate, NotificationQueryDto query);
        Task<ServiceResult<List<AllUserContractTaskBadge>>> GetAllContractNotificationBadgeByUserIdAsync(AuthenticateDto authenticate);
        Task<ServiceResult<UserEventsBadgeDto>> GetAllContractEventsBadgeByUserIdAsync(AuthenticateDto authenticate);
        Task<ServiceResult<bool>> AddScmAuditLogAndTaskAsync(AddAuditLogDto auditLogModel, AddTaskNotificationDto taskModel);
        Task<ServiceResult<bool>> AddScmAuditLogAsync(AddAuditLogDto AddModel, List<NotifToDto> notifTo);
        Task<ServiceResult<bool>> AddScmAuditLogAsync(AddAuditLogDto AddModel, int productGroupId, NotifEvent notifEvent, int? userId);
        Task<ServiceResult<bool>> AddScmAuditLogAsync(AddAuditLogDto AddModel, int productGroupId, List<NotifToDto> notifTo);
        Task<ServiceResult<bool>> AddDocumentAuditLogAsync(AddAuditLogDto AddModel, int documentGroupId, List<NotifToDto> notifTo);
        Task<ServiceResult<bool>> AddOperationAuditLogAsync(AddAuditLogDto AddModel, int OperaitonGroupId, List<NotifToDto> notifTo);


        Task<ServiceResult<bool>> AddPendingPOTaskNotificationAsync(AddAuditLogDto AddModel, NotifToDto notifTo,
          string contractCode, List<long> poIds);



        Task<ServiceResult<bool>> AddDocumentNotificationOnCreateContract(AuthenticateDto authenticate, string contractCode,
            List<DocumentGroup> documentGroups, NotifEvent notifEvent, List<string> roles);



        Task<ServiceResult<bool>> AddScmAuditLogAsync(List<AddAuditLogDto> AddModels, List<string> sendNotifRoles);
        Task<ServiceResult<bool>> AddScmAuditLogAsync(AddAuditLogDto AddModel, NotifEvent notifEvent, int? userId);
        Task<ServiceResult<bool>> AddScmAuditLogAsync(List<AddAuditLogDto> AddModels,List<AddTaskNotificationDto> notifTo,bool notif);
        string SerializerObject(object obj);

        Task<ServiceResult<bool>> SetDonedNotificationAsync(int userId, string keyValue, NotifEvent notifEvent);
        Task<ServiceResult<bool>> SetDonedNotificationAsync(string contractCode,string  formCode, string keyValue, NotifEvent notifEvent);

        Task<ServiceResult<bool>> SetDonedNotificationAsync(int userId, string contractCode, string keyValue, NotifEvent notifEvent);

        Task<ServiceResult<bool>> SetDonedNotificationAsync(int userId, string contractCode, string keyValue, string quantity, NotifEvent notifEvent);

        Task<ServiceResult<bool>> SetDonedNotificationByRootKeyValueAsync(int userId, string contractCode, string rootKeyValue, NotifEvent notifEvent);

        Task<ServiceResult<bool>> SetDonedNotificationAsync(int userId, string contractCode, List<string> keyValues, NotifEvent notifEvent);

        Task<ServiceResult<bool>> SetDonedNotificationAsync(int userId, List<string> contractCodes, NotifEvent notifEvent);

        Task<ServiceResult<bool>> RemoveNotificationAsync(string contractCode, string keyValue, NotifEvent notifEvent);
       
    }
}
