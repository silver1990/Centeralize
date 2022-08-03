using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.Document.RevisionConfirmation;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.DocumentManagment.Controllers
{
    [Route("api/Document/v1")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "documentManagement")]
    public class RevisionConfirmationController : ControllerBase
    {
        private readonly IRevisionConfirmationService _revisionConfirmationService;
        private readonly ILogger<RevisionConfirmationController> _logger;

        public RevisionConfirmationController(
            IRevisionConfirmationService revisionConfirmationService,
            ILogger<RevisionConfirmationController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _revisionConfirmationService = revisionConfirmationService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// get Confirmation user list
        /// </summary>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DocumentRevision/{revisionId:long}/ConfirmationUser")]
        public async Task<object> GetConfirmationUserListAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionObs,
                SCMRole.RevisionGlbObs,
                SCMRole.RevisionConfirmMng,
                SCMRole.RevisionConfirmGlbMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionConfirmationService.GetConfirmationUserListAsync(authenticate, revisionId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        ///  set or create Confirmation Revision 
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="revisionId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("Document/{documentId:long}/Revision/{revisionId:long}/confirmation")]
        public async Task<object> SetConfirmationRevisionAsync(long documentId, long revisionId, [FromBody] AddRevisionConfirmationDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.RevisionMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionConfirmationService.SetConfirmationRevisionAsync(authenticate, documentId, revisionId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// approve or reject confirm document Revision
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Todo
        ///     {
        ///         "isAccept": "doc-transmital",
        ///         "note": "some description ...."
        ///     }
        ///    
        /// </remarks>
        /// <param name="revisionId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("Revision/{revisionId:long}/userConfirmationTask")]
        public async Task<object> SetUserConfirmOwnRevisionTaskAsync(long revisionId, [FromBody] AddConfirmationAnswerDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.RevisionConfirmMng, SCMRole.RevisionConfirmGlbMng};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionConfirmationService.SetUserConfirmOwnRevisionTaskAsync(authenticate, revisionId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Pending Confirm Revision List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/pendingConfirm")]
        public async Task<object> GetPendingConfirmRevisionAsync([FromQuery] DocRevisionQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionObs,
                SCMRole.RevisionGlbObs,
                SCMRole.RevisionConfirmMng,
                SCMRole.RevisionConfirmGlbMng,
                SCMRole.RevisionCreator
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionConfirmationService.GetPendingConfirmRevisionAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Pending Confiem Revision Item by revisionId
        /// </summary>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/{revisionId:long}/pendingConfirm")]
        public async Task<object> GetPendingConfiemRevisionByRevIdAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionObs,
                SCMRole.RevisionGlbObs,
                SCMRole.RevisionConfirmMng,
                SCMRole.RevisionConfirmGlbMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionConfirmationService.GetPendingConfirmRevisionByRevIdAsync(authenticate, revisionId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get history of all confirmation 
        /// </summary>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/{revisionId:long}/history")]
        public async Task<object> GetReportRevisionConfirmationWorkFlowUSerByRevIdAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionObs,
                SCMRole.RevisionGlbObs,
                SCMRole.RevisionConfirmMng,
                SCMRole.RevisionConfirmGlbMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionConfirmationService.GetReportRevisionConfirmationWorkFlowUserByRevIdAsync(authenticate, revisionId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Download final Revision File 
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="revisionId"></param>
        /// <param name="attachType"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Document/{documentId:long}/Revision/{revisionId:long}/Confirm/downloadFile")]
        public async Task<object> DownloadRevisionNativeAndFinalFileAsync(long documentId, long revisionId, RevisionAttachmentType attachType, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionObs,
                SCMRole.RevisionGlbObs,
                SCMRole.RevisionConfirmMng,
                SCMRole.RevisionConfirmGlbMng,
            };
            var acceptType = new List<RevisionAttachmentType> { RevisionAttachmentType.Final, RevisionAttachmentType.FinalNative, RevisionAttachmentType.Preparation };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            if (!acceptType.Contains(attachType))
                return BadRequest();

            var streamResult = await _revisionConfirmationService.DownloadRevisionNativeAndFinalFileAsync(authenticate, documentId, revisionId, attachType, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, "application/octet-stream");
        }
    }
}
