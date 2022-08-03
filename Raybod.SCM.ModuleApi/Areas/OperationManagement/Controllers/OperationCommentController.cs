using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.OperationComment;
using Raybod.SCM.DataTransferObject.User;
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

namespace Raybod.SCM.ModuleApi.Areas.OperationManagement.Controllers
{
    [Route("api/Operation/v1/Operation")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "operationManagement")]
    public class OperationCommentController : ControllerBase
    {

        private readonly IOperationCommentService _operationCommentService;
        private readonly IUserService _userService;

        private readonly ILogger<OperationCommentController> _logger;

        public OperationCommentController(
            IOperationCommentService operationCommentService,
            IUserService userService,
            ILogger<OperationCommentController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _operationCommentService = operationCommentService;
            _userService = userService;
            _logger = logger;
           
               
           

        }


        /// <summary>
        /// Get operation comments
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("{operationId:Guid}/OperationComment")]
        [HttpGet]
        public async Task<object> GetOperationComment( Guid operationId, [FromQuery] OperationCommentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.RevisionMng,
            //    SCMRole.RevisionObs,
            //    SCMRole.RevisionActivityMng,
            //    SCMRole.DocumentArchiveObs,
            //    SCMRole.DocumentArchiveLimitedObs
            //};
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationCommentService.GetOperationCommentAsync(authenticate, operationId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Add operation comment
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("{operationId:Guid}/OperationComment")]
        [HttpPost]
        public async Task<object> AddOperationCommentAsync(Guid operationId, [FromBody] AddOperationCommentDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            //authenticate.Roles = new List<string> {
            //    SCMRole.RevisionMng,
            //    SCMRole.RevisionObs,
            //    SCMRole.RevisionActivityMng,
            //    SCMRole.DocumentArchiveObs,
            //    SCMRole.DocumentArchiveLimitedObs
            //};
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationCommentService.AddOperationCommentAsync(authenticate, operationId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Get operation comment files
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="commentId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{operationId:Guid}/OperationComment/{commentId:long}/downloadFile")]
        public async Task<object> DownloadCommentFileAsync(Guid operationId, long commentId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.RevisionMng,
            //    SCMRole.RevisionObs,
            //    SCMRole.RevisionActivityMng,
            //    SCMRole.DocumentArchiveObs,
            //    SCMRole.DocumentArchiveLimitedObs
            //};
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _operationCommentService.DownloadCommentFileAsync(authenticate, operationId, commentId, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType,streamResult.FileName);
        }

        /// <summary>
        /// Get users for mention in operation comments
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("OperationComment/userMention")]
        public async Task<object> GetUserMentionOfOperationCommentAsync( [FromQuery] UserQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.RevisionMng,
            //    SCMRole.RevisionObs,
            //    SCMRole.RevisionActivityMng,
            //    SCMRole.DocumentArchiveObs,
            //    SCMRole.DocumentArchiveLimitedObs
            //};
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _userService.GetUserMiniInfoWithoutAuthenticationAsync(query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
