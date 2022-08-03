using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Raybod.SCM.DataTransferObject.Document.Communication;
using Raybod.SCM.DataTransferObject.Document.Communication.TQ;
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
    public class CommunicationTQController : ControllerBase
    {
        private readonly ICommunicationTQService _communicationTQService;
        private readonly ICommunicationTeamCommentService _communicationTeamCommentService;
        private readonly IUserService _userService;
        private readonly ILogger<CommunicationTQController> _logger;
        private readonly IDocumentCommunicationService _documentCommunicationService;

        public CommunicationTQController(
            ICommunicationTQService communicationTQService,
              ICommunicationTeamCommentService communicationTeamCommentService,
            IUserService userService,
            ILogger<CommunicationTQController> logger,IHttpContextAccessor httpContextAccessor,
            IDocumentCommunicationService documentCommunicationService
            )
        {
            _communicationTQService = communicationTQService;
            _communicationTeamCommentService = communicationTeamCommentService;
            _userService = userService;
            _logger=logger;
            _documentCommunicationService = documentCommunicationService;
           
            
                
           
           
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

            authenticate.Roles = new List<string> {
                SCMRole.TQReg,
                SCMRole.TQMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationTQService.AddCommunicationTQAsync(authenticate, revisionId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
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
            authenticate.Roles = new List<string> {
                SCMRole.TQLimitedObs,
                SCMRole.TQLimitedGlbObs,
                SCMRole.TQLimitedGlbReply,
                SCMRole.TQObs,
                SCMRole.TQReg,
                SCMRole.TQReply,
                SCMRole.TQMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationTQService.GetTQListAsync(authenticate, query);
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
            authenticate.Roles = new List<string> {
                SCMRole.TQLimitedObs,
                SCMRole.TQLimitedGlbObs,
                SCMRole.TQLimitedGlbReply,
                SCMRole.TQObs,
                SCMRole.TQReg,
                SCMRole.TQReply,
                SCMRole.TQMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationTQService.GetTQQuestionDetailsAsync(authenticate, communicationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get communication TQ Details
        /// </summary>
        /// <param name="communicationId"></param>
        /// <returns></returns>
        [HttpGet, Route("{communicationId:long}/TQDetails")]
        public async Task<object> GetTQDetailsAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> { SCMRole.RevisionMng, SCMRole.RevisionObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationTQService.GetTQDetailsAsync(authenticate, communicationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add communication TQ question Reply 
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{communicationId:long}/TQ/reply")]
        public async Task<object> AddReplayTQQuestionAsync(long communicationId, [FromBody] AddTQReplyDto model)
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
                SCMRole.TQReply,
                SCMRole.TQLimitedGlbReply,
                SCMRole.TQMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationTQService.AddReplayTQQuestionAsync(authenticate, communicationId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add Communication TQ shareing Attachment
        /// </summary>
        /// <param name="communicationId"></param>
        /// <returns></returns>
        [HttpPost, Route("{communicationId:long}/TQ/shareingFile")]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<object> AddCommunicationTQAttachmentAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.TQReg,
                SCMRole.TQReply,
                SCMRole.TQMng,
                SCMRole.TQLimitedGlbReply,
            };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var files = HttpContext.Request.Form.Files;
            var serviceResult = await _communicationTQService.AddCommunicationTQAttachmentAsync(authenticate, communicationId, files);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        [HttpGet]
        [Route("{communicationId:long}/TQ/generatePdf")]
        public async Task<object> GenerateTQPdfAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.TransmittalMng,
            //    SCMRole.TransmittalObs
            //};
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var ArchiveFileResult = await _communicationTQService.GenerateTQPdfAsync(authenticate, communicationId);
            if (ArchiveFileResult == null)
                return NotFound();

            return File(ArchiveFileResult.ArchiveFile, "application/octet-stream");
            //return File(ArchiveFileResult.ArchiveFile, ArchiveFileResult.ContentType, ArchiveFileResult.FileName);
        }
        /// <summary>
        /// Delete Communication TQ shareing Attachment Async
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpDelete, Route("{communicationId:long}/TQ/shareingFile/{fileSrc}")]
        public async Task<object> DeleteCommunicationTQAttachmentAsync(long communicationId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.TQReg,
                SCMRole.TQReply,
                SCMRole.TQMng,
                SCMRole.TQLimitedGlbReply,
            };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationTQService.DeleteCommunicationTQAttachmentAsync(authenticate, communicationId, fileSrc);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download TQ Attachment
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{communicationId:long}/TQ/downloadFile")]
        public async Task<object> DownloadTQFileAsync(long communicationId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.TransmittalMng,
            //    SCMRole.TransmittalObs
            //};


            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _communicationTQService.DownloadTQFileAsync(authenticate, communicationId, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, "application/octet-stream");
        }

        /// <summary>
        /// Add TQ Team Comment
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{communicationId:long}/TQ/TeamComment")]
        public async Task<object> AddTQTeamCommentAsync(long communicationId, [FromBody] AddCommunicationTeamCommentDto model)
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
            //    SCMRole.TQLimitedObs,
            //    SCMRole.TQObs,
            //    SCMRole.TQReg,
            //    SCMRole.TQReply,
            //    SCMRole.TQMng,
            //};
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationTeamCommentService.AddTQAndNCRCommentAsync(authenticate, communicationId, CommunicationType.TQ, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get TQ Team Comment 
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("{communicationId:long}/TQ/TeamComment")]
        public async Task<object> GetTQTeamCommentAsync(long communicationId, [FromQuery] CommunicationTeamCommentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.TQLimitedObs,
            //    SCMRole.TQObs,
            //    SCMRole.TQReg,
            //    SCMRole.TQReply,
            //    SCMRole.TQMng,
            //};
            query.Type = CommunicationType.TQ;
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationTeamCommentService.GetTQAndNCRCommentAsync(authenticate, communicationId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get User Mention Of TQ Comment 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("TQ/userMention")]
        public async Task<object> GetUserMentionOfTQCommentAsync([FromQuery] UserQueryDto query)
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
        [Route("{communicationId:long}/TQ/downloadAttachments")]
        public async Task<object> GenerateTQAttachmentZipAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.TransmittalMng,
            //    SCMRole.TransmittalObs
            //};
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var ArchiveFileResult = await _communicationTQService.GenerateTQAttachmentZipAsync(authenticate, communicationId);
            if (ArchiveFileResult == null)
                return NotFound();

            return File(ArchiveFileResult.Stream, ArchiveFileResult.ContentType,ArchiveFileResult.FileName);
            //return File(ArchiveFileResult.ArchiveFile, ArchiveFileResult.ContentType, ArchiveFileResult.FileName);
        }
        [HttpGet, Route("TQ/TQpendingReply")]
        public async Task<object> GetPendingCommunicationListAsync([FromQuery] CommunicationQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> { SCMRole.RevisionMng, SCMRole.RevisionObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentCommunicationService.GetPendingReplyCommunicationListAsync(authenticate, query,CommunicationType.TQ);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Download tq comment and reply attachment
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="commentId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{communicationId:long}/tqCommentAndReply/{commentId:long}/downloadFile")]
        public async Task<object> DownloadTQCommentAndReplyFileAsync(long communicationId, long commentId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.TransmittalMng,
            //    SCMRole.TransmittalObs
            //};


            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _communicationTQService.DownloadTQFileAsync(authenticate, communicationId, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType,streamResult.FileName);
        }
    }
    
}