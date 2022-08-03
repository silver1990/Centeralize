using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Utility.Filters;
using Raybod.SCM.DataTransferObject.User;
using System;


namespace Raybod.SCM.ModuleApi.Controllers
{
    [Route("api/messages")]
    //[Authorize]
    [ApiController]
    [SwaggerArea(AreaName = "raybodPanel")]

    public class MessagesController : ControllerBase
    {
        private readonly ISCMLogAndNotificationService _scmLogAndNotificationService;
        private readonly IUserService _userService;

        public MessagesController(
            ISCMLogAndNotificationService scmLogAndNotificationService,
            IUserService userService)
        {
            _scmLogAndNotificationService = scmLogAndNotificationService;
            _userService = userService;
        }

        /// <summary>
        /// Get User Notification
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("notification")]
        public async Task<object> GetUserNotification([FromQuery] NotificationQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var serviceResult = await _scmLogAndNotificationService.GetNotificationByUserIdAsync(authenticate, query);
            return serviceResult.ToHttpResponseV2();
        }

        
        /// <summary>
        /// set seen user notifications
        /// </summary>
        /// <param name="notificationId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("notification/SetSeen/{notificationId}")]
        public async Task<object> SetSeenUserNotificationAsync(Guid notificationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var serviceResult = await _scmLogAndNotificationService.SetSeenUserNotificationAsync(authenticate, notificationId);
            return serviceResult.ToHttpResponseV2();
        }


        /// <summary>
        /// change user notitfication pin state
        /// </summary>
        /// <param name="notificationId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("notification/changePinState/{notificationId}")]
        public async Task<object> UpdatePinNotificationAsync(Guid notificationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var serviceResult = await _scmLogAndNotificationService.UpdatePinNotificationAsync(authenticate, notificationId);
            return serviceResult.ToHttpResponseV2();
        }

        /// <summary>
        /// set seen user notifications by userId
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("notification/checkAll")]
        public async Task<object> SetSeenUserNotificationByUserIdAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var serviceResult = await _scmLogAndNotificationService.SetSeenUserNotificationByUserIdAsync(authenticate);
            return serviceResult.ToHttpResponseV2();
        }
        /// <summary>
        /// Get current project event By UserId
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("event")]
        public async Task<object> GetAuditlogByUserIdAndPermission([FromQuery] AuditLogQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var serviceResult = await _scmLogAndNotificationService.GetAuditlogByUserIdAndPermission(authenticate, query);
            return serviceResult.ToHttpResponseV2();
        }

        /// <summary>
        /// Set Seen eveby By UserId 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("event/SetSeenEvents")]
        public async Task<object> SetSeenNotification()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var serviceResult = await _scmLogAndNotificationService.SetSeenLogNotificationByUserIdAsync(authenticate);
            return serviceResult.ToHttpResponseV2();
        }
        /// <summary>
        /// change user event pin state
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("event/changePinState/{eventId}")]
        public async Task<object> UpdatePinEventByUserIdAsync(Guid eventId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var serviceResult = await _scmLogAndNotificationService.UpdatePinEventByUserIdAsync(authenticate, eventId);
            return serviceResult.ToHttpResponseV2();
        }
        /// <summary>
        /// Get User  Mention
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("mentions")]
        public async Task<object> GetUserMentionNotification([FromQuery] NotificationQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var serviceResult = await _scmLogAndNotificationService.GetMentionNotificationByUserIdAsync(authenticate, query);
            return serviceResult.ToHttpResponseV2();
        }

        /// <summary>
        /// set seen user mentions
        /// </summary>
        /// <param name="mentionId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("mention/SetSeen/{mentionId}")]
        public async Task<object> SetSeenMentionNotificationAsync(Guid mentionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var serviceResult = await _scmLogAndNotificationService.SetSeenMentionNotificationAsync(authenticate, mentionId);
            return serviceResult.ToHttpResponseV2();
        }

        /// <summary>
        /// change user mention pin state
        /// </summary>
        /// <param name="mentionId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("mention/changePinState/{mentionId}")]
        public async Task<object> UpdatePinMentionNotificationAsync(Guid mentionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var serviceResult = await _scmLogAndNotificationService.UpdatePinMentionNotificationAsync(authenticate, mentionId);
            return serviceResult.ToHttpResponseV2();
        }
        /// <summary>
        /// Set Seen mention By UserId 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("mention/checkAll")]
        public async Task<object> SetSeenMentions()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var serviceResult = await _scmLogAndNotificationService.SetSeenMentionByUserIdAsync(authenticate);
            return serviceResult.ToHttpResponseV2();
        }

        [HttpGet]
        [Route("allMessagesBadge")]
        public async Task<object> GetAllContractEventsBadgeByUserIdAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var serviceResult = await _scmLogAndNotificationService.GetAllContractEventsBadgeByUserIdAsync(authenticate);
            return serviceResult.ToHttpResponseV2();
        }
    }


}