using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Document.Communication.TQ;
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
    public class CustomerCommunicationTQController : ControllerBase
    {
        private readonly ICommunicationTQService _communicationTQService;
        private readonly IUserService _userService;
        private readonly ILogger<CustomerCommunicationTQController> _logger;

        public CustomerCommunicationTQController(
            ICommunicationTQService communicationTQService,
            ILogger<CustomerCommunicationTQController> logger,IHttpContextAccessor httpContextAccessor,
            IUserService userService)
        {
            _communicationTQService = communicationTQService;
            _logger = logger;
            _userService = userService;
           
               
           
        }

        /// <summary>
        /// Get Communication TQ List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("TQ")]
        public async Task<object> GetTQListAsync([FromQuery] TQQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationTQService.GetTQListForCustomerAsync(authenticate, query,accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get communication TQ question details
        /// </summary>
        /// <param name="communicationId"></param>
        /// <returns></returns>
        [HttpGet, Route("{communicationId:long}/TQ")]
        public async Task<object> GetTQQuestionDetailsAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationTQService.GetTQQuestionDetailsForCustomerAsync(authenticate, communicationId,accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Add Communication TQ 
        /// </summary>
        /// <param name="revisionId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("TQ/{revisionId:long}")]
        public async Task<object> AddCommunicationTQAsync(long revisionId, [FromBody] AddTQDto model)
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
            var serviceResult = await _communicationTQService.AddCommunicationTQForCustomerAsync(authenticate, revisionId, model,accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
