using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Document.Communication.NCR;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.ModuleApi.Areas.SCMCustomerUserManagement.Controllers
{

    [Route("api/SCMCustomerDocumentManagement/v1/communication")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "SCMCustomerPanelManagement")]
    public class CustomerCommunicationNCRController : ControllerBase
    {

        private readonly ICommunicationNCRService _communicationNCRService;
        private readonly ICommunicationTeamCommentService _communicationTeamCommentService;
        private readonly IUserService _userService;
        private readonly ILogger<CustomerCommunicationNCRController> _logger;
        private readonly IDocumentCommunicationService _documentCommunicationService;

        public CustomerCommunicationNCRController(
            ICommunicationNCRService communicationNCRService,
             ICommunicationTeamCommentService communicationTeamCommentService,
             IUserService userService,
             ILogger<CustomerCommunicationNCRController> logger,IHttpContextAccessor httpContextAccessor,
             IDocumentCommunicationService documentCommunicationService)
        {
            _communicationNCRService = communicationNCRService;
            _communicationTeamCommentService = communicationTeamCommentService;
            _userService = userService;
            _logger = logger;
            _documentCommunicationService = documentCommunicationService;
           
               
           
        }

        /// <summary>
        /// Get Communication NCR List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("NCR")]
        public async Task<object> GetNCRListAsync([FromQuery] NCRQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationNCRService.GetNCRListForCustomerAsync(authenticate, query, accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get communication NCR question details
        /// </summary>
        /// <param name="communicationId"></param>
        /// <returns></returns>
        [HttpGet, Route("{communicationId:long}/NCR")]
        public async Task<object> GetNCRQuestionDetailsAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationNCRService.GetNCRQuestionDetailsForCustomerUserAsync(authenticate, communicationId, accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add Communication NCR 
        /// </summary>
        /// <param name="revisionId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("NCR/{revisionId:long}")]
        public async Task<object> AddCommunicationNCRAsync(long revisionId, [FromBody] AddNCRDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationNCRService.AddCommunicationNCRForCustomerUserAsync(authenticate, revisionId, model, accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
