using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;


namespace Raybod.SCM.ModuleApi.Areas.DocumentManagment.Controllers
{
    [Route("api/Document/v1/Document/{documentId:long}/Revision")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "documentManagement")]
    public class RevisionCommentController : ControllerBase
    {
        private readonly IRevisionCommentService _revisionCommentService;
        private readonly IUserService _userService;

        private readonly ILogger<RevisionCommentController> _logger;

        public RevisionCommentController(
            IRevisionCommentService revisionCommentService,
            IUserService userService,
            ILogger<RevisionCommentController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _revisionCommentService = revisionCommentService;
            _userService = userService;
            _logger = logger;
           
        }


        [Route("{revisionId:long}/RevisionComment")]
        [HttpGet]
        public async Task<object> GetRevisionComment(long documentId, long revisionId, [FromQuery] RevisionCommentQueryDto query)
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
            var serviceResult = await _revisionCommentService.GetRevisionCommentAsync(authenticate, documentId, revisionId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        //[Route("{revisionId:long}/RevisionComment/{revisionCommentId:long}")]
        //[HttpDelete]
        //public async Task<object> RemoveRevisionCommentAsync(long documentId, long revisionId, long revisionCommentId)
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> {
        //        SCMRole.RevisionMng,
        //        SCMRole.RevisionObs,
        //        SCMRole.RevisionActivityMng,
        //        SCMRole.DocumentArchiveObs,
        //        SCMRole.DocumentArchiveLimitedObs
        //    };

        //    var serviceResult = await _revisionCommentService.RemoveRFPCommentByIdAsync(authenticate, documentId, revisionId, revisionCommentId);
        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}

        [Route("{revisionId:long}/RevisionComment")]
        [HttpPost]
        public async Task<object> AddRevisionCommentAsync(long documentId, long revisionId, [FromBody] AddRevisionCommentDto model)
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
            var serviceResult = await _revisionCommentService.AddRevisionCommentAsync(authenticate, documentId, revisionId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet]
        [Route("{revisionId:long}/RevisionComment/{commentId:long}/downloadFile")]
        public async Task<object> DownloadCommentFileAsync(long documentId, long revisionId, long commentId, string fileSrc)
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
            var streamResult = await _revisionCommentService.DownloadCommentFileAsync(authenticate, documentId, revisionId, commentId, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType,streamResult.FileName);
        }


        [HttpGet]
        [Route("{revisionId:long}/RevisionComment/userMention")]
        public async Task<object> GetUserMentionOfRevisionCommentAsync(long documentId, long revisionId, [FromQuery] UserQueryDto query)
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