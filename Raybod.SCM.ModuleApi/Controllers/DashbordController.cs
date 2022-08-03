using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Dashboard;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.ModuleApi.Helper.Authentication;

namespace Raybod.SCM.ModuleApi.Controllers
{
    [Route("api/raybodSCM/v1/dashbord")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "raybodPanel")]
    public class DashbordController : ControllerBase
    {
        private IBomProductService _bomProductService;
        private IPurchaseRequestService _purchaseRequestService;
        private IRFPService _rfpService;
        private IPOService _poService;
        private IMasterMrService _masterMrService;
        private IDocumentRevisionService _documentRevisionService;
        private IDocumentCommunicationService _documentCommunicationService;
        private ITeamWorkAuthenticationService _teamWorkAuthenticationService;
        private IOperationService _operationService;
        private IUserService _userService;
        private IUserNotifyService _notifyService;
        private readonly IAuthenticate _authenticate;
        private readonly ILogger<DashbordController> _logger;

        public DashbordController(
            IBomProductService bomProductService,
            IPurchaseRequestService purchaseRequestService,
            IRFPService rfpService,
            IPOService poService,
            IDocumentRevisionService documentRevisionService,
            IDocumentCommunicationService documentCommunicationService,
            ITeamWorkAuthenticationService teamWorkAuthenticationService,
            IUserService userService,
            IMasterMrService masterMrService,
            ILogger<DashbordController> logger,
            IOperationService operationService, IUserNotifyService notifyService, IAuthenticate authenticate)
        {
            _purchaseRequestService = purchaseRequestService;
            _bomProductService = bomProductService;
            _rfpService = rfpService;
            _poService = poService;
            _masterMrService = masterMrService;
            _documentRevisionService = documentRevisionService;
            _documentCommunicationService = documentCommunicationService;
            _teamWorkAuthenticationService = teamWorkAuthenticationService;
            _userService = userService;
            _logger = logger;
            _operationService = operationService;
            _notifyService = notifyService;
            _authenticate = authenticate;
        }

        /// <summary>
        /// get dashboard form badge count
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("scmFormCounter")]
        public async Task<object> GetSCMFormCounterAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var counter = new SCMFormCounterDto();

            authenticate.Roles = new List<string> { SCMRole.BomMng, SCMRole.BomObs };
            counter.PendingBOM = 0;

            authenticate.Roles = new List<string> { SCMRole.MrpMng, SCMRole.MrpObs };
            counter.PendingMRP = await _masterMrService.DashbourdWaitingContractForMrpBadgeCountAsync(authenticate);

            var prCounterResult = await _purchaseRequestService.GetDashbourdWaitingListBadgeCountAsync(authenticate);
            counter.PendingApprovePR = prCounterResult.WaitingForConfirmQuantity;
            counter.PendingPR = prCounterResult.WaitingForNewPRQuantity;

            authenticate.Roles = new List<string> { SCMRole.RFPMng, SCMRole.RFPObs };
            var rfpBadge = await _rfpService.GetDashbourdRFPListBadgeCountAsync(authenticate);
            counter.PendingRFP = rfpBadge.PenddingRFP;
            counter.InprogressRFP = rfpBadge.InprogressRFP;

            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };
            var poBadge = await _poService.GetDashbourdPOListBadgeAsync(authenticate);
            counter.PendingPo = poBadge.PenddingPo;
            counter.InprogressPO = poBadge.InprogressPo;

            var inprogressOperationBadge = await _operationService.GetInProgressOperationBadgeCountAsync(authenticate);
            counter.InprogressOperation = inprogressOperationBadge.Result;

            var revisionBadge = await _documentRevisionService.GetRevisionDashboardBadgeAsync(authenticate);
            counter.InProgressRevision = revisionBadge.InProgressRevision;
            counter.PendingConfirmationRevision = revisionBadge.PendingConfirmationRevision;
            counter.PendingTransmittalRevision = revisionBadge.PendingTransmittalRevision;

            var communicationBadge = await _documentCommunicationService.GetPendingReplyCommunicationBadgeForDashbourdAsync(authenticate);
            counter.PendingCommunicationReply = communicationBadge.Result.Sum(r => r.Value);
            counter.PendingCommentReply = communicationBadge.Result[1];
            counter.PendingTQReply = communicationBadge.Result[2];
            counter.PendingNCRReply = communicationBadge.Result[3];

            return
                new ServiceResult<SCMFormCounterDto>(true, counter,
                        new List<ServiceMessage> { new ServiceMessage(MessageType.Succeed, MessageId.Succeeded) })
                    .ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// User Change Password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("user/changePassword")]
        public async Task<object> UserChangePasswordDto([FromBody] UserChangePasswordDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _userService.ChangePasswordAsync(authenticate.UserId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
       
        /// <summary>
        /// get User Permission 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("user/userPermision")]
        public async Task<object> GetUserPermissionByUserIdAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _teamWorkAuthenticationService.GetUserPermissionByUserIdAsync(authenticate.UserId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet]
        [Route("remoteIP")]
        public async Task<object> GetremoteIp()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var ip = $"RemoteIpAddress:{Request.HttpContext.Connection.RemoteIpAddress},LocalIpAddress:{Request.HttpContext.Connection.LocalIpAddress},HttpContext.Connection.RemoteIpAddress:{HttpContext.Connection.RemotePort},authenticate.RemoteIpAddress{authenticate.RemoteIpAddress}";
            return Ok(ip);
        }

        [HttpGet, Route("userNotify")]
        public async Task<object> GetUserEmailNotifiesAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _notifyService.GetUserNotifies(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpPost, Route("userNotify")]
        public async Task<object> UpdateUserEmailNotifiesAsync([FromBody] UserNotifyListDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _notifyService.UpdateUserNotifies(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

    }
}