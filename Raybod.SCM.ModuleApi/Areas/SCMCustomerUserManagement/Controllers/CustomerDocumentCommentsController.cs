using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.Document.Communication;
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
    [Route("api/SCMCustomerDocumentManagement/v1/CustomerDocumentComments")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "SCMCustomerPanelManagement")]
    public class CustomerDocumentCommentsController : ControllerBase
    {

        private readonly ICommunicationCommentService _communicationCommentService;
        private readonly IUserService _userService;
        private readonly ILogger<CustomerDocumentCommentsController> _logger;
        private readonly IDocumentService _documentService;

        public CustomerDocumentCommentsController(
            ICommunicationCommentService communicationCommentService,
            IUserService userService,
            ILogger<CustomerDocumentCommentsController> logger,IHttpContextAccessor httpContextAccessor,
            IDocumentService documentService)
        {
            _communicationCommentService = communicationCommentService;
            _userService = userService;
            _logger = logger;
            _documentService = documentService;
           
               
           
        }
        /// <summary>
        /// Get Communication comment List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("Comments")]
        public async Task<object> GetCommunicationCommentListAsync([FromQuery] COMQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationCommentService.GetCommunicationCommentListForCustomerUserAsync(authenticate, query,accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Document list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("PendingForCommentDocuments")]
        public async Task<object> GetIFADocumentsAsync([FromQuery] DocumentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentService.GetPenndingDocumentForCommentsAsync(authenticate, query, true, true,accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet, Route("LastTransmittalRevision/{revisionId:long}")]
        public async Task<object> GetDocumentByIdAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerAccess(authenticate);
            var serviceResult = await _documentService.GetLastTransmittalRevisionForDocumentAsync(authenticate, revisionId,accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add Communication Comment 
        /// </summary>
        /// <param name="revisionId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("comment/{revisionId:long}")]
        public async Task<object> AddCommunicationCommentAsync(long revisionId, [FromBody] AddCommunicationCommentDto model)
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
            var serviceResult = await _communicationCommentService.AddCommunicationCommentForCustomerUserAsync(authenticate, revisionId, model,accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

       
        /// <summary>
        /// get communication comment question details
        /// </summary>
        /// <param name="communicationId"></param>
        /// <returns></returns>
        [HttpGet, Route("{communicationId:long}/comment")]
        public async Task<object> GetCommentQuestionDetailsAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationCommentService.GetCommentQuestionDetailsForCustomerUserAsync(authenticate, communicationId,accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


    }
}
