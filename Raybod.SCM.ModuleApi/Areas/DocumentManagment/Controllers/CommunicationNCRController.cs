using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Raybod.SCM.DataTransferObject.Document.Communication;
using Raybod.SCM.DataTransferObject.Document.Communication.NCR;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;

namespace Raybod.SCM.ModuleApi.Areas.DocumentManagment.Controllers
{
    [Route("api/Document/v1/communication")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "documentManagement")]
    public class CommunicationNCRController : ControllerBase
    {
        private readonly ICommunicationNCRService _communicationNCRService;
        private readonly ICommunicationTeamCommentService _communicationTeamCommentService;
        private readonly IUserService _userService;
        private readonly ILogger<CommunicationNCRController> _logger;
        private readonly IDocumentCommunicationService _documentCommunicationService;
        private string language="";
        public CommunicationNCRController(
            ICommunicationNCRService communicationNCRService,
             ICommunicationTeamCommentService communicationTeamCommentService,
            IUserService userService,
             ILogger<CommunicationNCRController> logger,IHttpContextAccessor httpContextAccessor,
             IDocumentCommunicationService documentCommunicationService)
        {
            _communicationNCRService = communicationNCRService;
            _communicationTeamCommentService = communicationTeamCommentService;
            _userService = userService;
            _logger = logger;
            _documentCommunicationService = documentCommunicationService;
           
            
                
           
           
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


            authenticate.Roles = new List<string> {
                SCMRole.NCRReg,
                SCMRole.NCRMng
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationNCRService.AddCommunicationNCRAsync(authenticate, revisionId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
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
            authenticate.Roles = new List<string> {
                SCMRole.NCRLimitedObs,
                SCMRole.NCRLimitedGlbObs,
                SCMRole.NCRLimitedGlbReply,
                SCMRole.NCRObs,
                SCMRole.NCRReg,
                SCMRole.NCRReply,
                SCMRole.NCRMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationNCRService.GetNCRListAsync(authenticate, query);
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
            authenticate.Roles = new List<string> {
                SCMRole.NCRLimitedObs,
                SCMRole.NCRLimitedGlbObs,
                SCMRole.NCRLimitedGlbReply,
                SCMRole.NCRObs,
                SCMRole.NCRReg,
                SCMRole.NCRReply,
                SCMRole.NCRMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationNCRService.GetNCRQuestionDetailsAsync(authenticate, communicationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get communication NCR Details details
        /// </summary>
        /// <param name="communicationId"></param>
        /// <returns></returns>
        [HttpGet, Route("{communicationId:long}/NCRDetails")]
        public async Task<object> GetNCRDetailsAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> { SCMRole.RevisionMng, SCMRole.RevisionObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationNCRService.GetNCRDetailsAsync(authenticate, communicationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add communication NCR question Reply 
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{communicationId:long}/NCR/reply")]
        public async Task<object> AddReplayNCRQuestionAsync(long communicationId, [FromBody] AddNCRReplyDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> {
                SCMRole.NCRReply,
                SCMRole.NCRMng,
                SCMRole.NCRLimitedGlbReply,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationNCRService.AddReplayNCRQuestionAsync(authenticate, communicationId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add Communication NCR shareing Attachment
        /// </summary>
        /// <param name="communicationId"></param>
        /// <returns></returns>
        [HttpPost, Route("{communicationId:long}/NCR/shareingFile")]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<object> AddCommunicationNCRAttachmentAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.NCRReg,
                SCMRole.NCRReply,
                SCMRole.NCRLimitedGlbReply,
                SCMRole.NCRMng,
            };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var files = HttpContext.Request.Form.Files;
            var serviceResult = await _communicationNCRService.AddCommunicationNCRAttachmentAsync(authenticate, communicationId, files);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Delete Communication NCR shareing Attachment Async
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpDelete, Route("{communicationId:long}/NCR/shareingFile/{fileSrc}")]
        public async Task<object> DeleteCommunicationNCRAttachmentAsync(long communicationId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.NCRReg,
                SCMRole.NCRReply,
                SCMRole.NCRLimitedGlbReply,
                SCMRole.NCRMng,
            };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationNCRService.DeleteCommunicationNCRAttachmentAsync(authenticate, communicationId, fileSrc);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Download NCR Attachment
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{communicationId:long}/NCR/downloadFile")]
        public async Task<object> DownloadNCRFileAsync(long communicationId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.TransmittalMng,
            //    SCMRole.TransmittalObs
            //};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _communicationNCRService.DownloadNCRFileAsync(authenticate, communicationId, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, "application/octet-stream");
        }

        
        /// <summary>
        /// Add NCR Team Comment
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{communicationId:long}/NCR/TeamComment")]
        public async Task<object> AddNCRTeamCommentAsync(long communicationId, [FromBody] AddCommunicationTeamCommentDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            //authenticate.Roles = new List<string> { SCMRole.RevisionMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationTeamCommentService.AddTQAndNCRCommentAsync(authenticate, communicationId, CommunicationType.NCR, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        [HttpGet]
        [Route("{communicationId:long}/NCR/generatePdf")]
        public async Task<object> GenerateNCRPdfAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.TransmittalMng,
            //    SCMRole.TransmittalObs
            //};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var ArchiveFileResult = await _communicationNCRService.GenerateNCRPdfAsync(authenticate, communicationId);
            if (ArchiveFileResult == null)
                return NotFound();

            return File(ArchiveFileResult.ArchiveFile, "application/octet-stream");
            //return File(ArchiveFileResult.ArchiveFile, ArchiveFileResult.ContentType, ArchiveFileResult.FileName);
        }
        /// <summary>
        /// Get NCR Team Comment 
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("{communicationId:long}/NCR/TeamComment")]
        public async Task<object> GetNCRTeamCommentAsync(long communicationId, [FromQuery] CommunicationTeamCommentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> { SCMRole.RevisionMng };

            query.Type = CommunicationType.NCR;

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationTeamCommentService.GetTQAndNCRCommentAsync(authenticate, communicationId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get UserMention Of NCR Comment 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("NCR/userMention")]
        public async Task<object> GetUserMentionOfNCRCommentAsync([FromQuery] UserQueryDto query)
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


        [HttpGet]
        [Route("{communicationId:long}/NCR/downloadAttachments")]
        public async Task<object> GenerateNCRAttachmentZipAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.TransmittalMng,
            //    SCMRole.TransmittalObs
            //};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var ArchiveFileResult = await _communicationNCRService.GenerateNCRAttachmentZipAsync(authenticate, communicationId);
            if (ArchiveFileResult == null)
                return NotFound();

            return File(ArchiveFileResult.Stream, ArchiveFileResult.ContentType,ArchiveFileResult.FileName);
            //return File(ArchiveFileResult.ArchiveFile, ArchiveFileResult.ContentType, ArchiveFileResult.FileName);
        }
        [HttpGet, Route("NCR/ncrPendingReply")]
        public async Task<object> GetPendingCommunicationListAsync([FromQuery] CommunicationQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> { SCMRole.RevisionMng, SCMRole.RevisionObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentCommunicationService.GetPendingReplyCommunicationListAsync(authenticate, query, CommunicationType.NCR);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download NCR Comment And Reply Attachment
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="commentId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{communicationId:long}/ncrCommentAndReply/{commentId:long}/downloadFile")]
        public async Task<object> DownloadNCRCommentAndReplyFileAsync(long communicationId,long commentId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.TransmittalMng,
            //    SCMRole.TransmittalObs
            //};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _communicationNCRService.DownloadNCRFileAsync(authenticate, communicationId, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType,streamResult.FileName);
        }
    }
}