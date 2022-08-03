using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Raybod.SCM.DataTransferObject.Document.Communication;
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
    public class CommunicationCommentController : ControllerBase
    {
        private readonly ICommunicationCommentService _communicationCommentService;
        private readonly ICommunicationTeamCommentService _communicationTeamCommentService;
        private readonly IUserService _userService;
        private readonly ILogger<CommunicationCommentController> _logger;
        private readonly IDocumentCommunicationService _documentCommunicationService;

        public CommunicationCommentController(
            ICommunicationCommentService communicationCommentService,
            ICommunicationTeamCommentService communicationTeamCommentService,
            IUserService userService,
             ILogger<CommunicationCommentController> logger,IHttpContextAccessor httpContextAccessor,
             IDocumentCommunicationService documentCommunicationService)
        {
            _communicationCommentService = communicationCommentService;
            _communicationTeamCommentService = communicationTeamCommentService;
            _userService = userService;
            _logger = logger;
            _documentCommunicationService = documentCommunicationService;
           
            
                
           
           
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


            authenticate.Roles = new List<string> {
                SCMRole.ComCommentReg,
                SCMRole.ComCommentMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationCommentService.AddCommunicationCommentAsync(authenticate, revisionId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Communication comment List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("Comment")]
        public async Task<object> GetCommunicationCommentListAsync([FromQuery] COMQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.ComCommentLimitedObs,
                SCMRole.ComCommentLimitedGlbObs,
                SCMRole.ComCommentLimitedGlbReply,
                SCMRole.ComCommentObs,
                SCMRole.ComCommentReg,
                SCMRole.ComCommentReply,
                SCMRole.ComCommentMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationCommentService.GetCommunicationCommentListAsync(authenticate, query);
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
            authenticate.Roles = new List<string> {
                SCMRole.ComCommentLimitedObs,
                SCMRole.ComCommentLimitedGlbObs,
                SCMRole.ComCommentLimitedGlbReply,
                SCMRole.ComCommentObs,
                SCMRole.ComCommentReg,
                SCMRole.ComCommentReply,
                SCMRole.ComCommentMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationCommentService.GetCommentQuestionDetailsAsync(authenticate, communicationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Comment Details 
        /// </summary>
        /// <param name="communicationId"></param>
        /// <returns></returns>
        [HttpGet, Route("{communicationId:long}/commentDetails")]
        public async Task<object> GetCommentDetailsAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> { SCMRole.RevisionMng, SCMRole.RevisionObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationCommentService.GetCommentDetailsAsync(authenticate, communicationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add communication comment question Reply 
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{communicationId:long}/comment/reply")]
        public async Task<object> AddReplyCommentAsync(long communicationId, [FromBody] ReplyCommunicationCommentDto model)
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
                SCMRole.ComCommentReply,
                SCMRole.ComCommentLimitedGlbReply,
                SCMRole.ComCommentMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationCommentService.AddReplyCommentAsync(authenticate, communicationId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        
        /// <summary>
        /// Add Communication Comment shareing Attachment
        /// </summary>
        /// <param name="communicationId"></param>
        /// <returns></returns>
        [HttpPost, Route("{communicationId:long}/comment/shareingFile")]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<object> AddCommunicationCommentAttachmentAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.ComCommentReg,
                SCMRole.ComCommentReply,
                SCMRole.ComCommentLimitedGlbReply,
                SCMRole.ComCommentMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var files = HttpContext.Request.Form.Files;
            var serviceResult = await _communicationCommentService.AddCommunicationCommentAttachmentAsync(authenticate, communicationId, files);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Delete Communication Comment shareing Attachment Async
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpDelete, Route("{communicationId:long}/comment/shareingFile/{fileSrc}")]
        public async Task<object> DeleteCommunicationCommentAttachmentAsync(long communicationId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.ComCommentReg,
                SCMRole.ComCommentReply,
                SCMRole.ComCommentLimitedGlbReply,
                SCMRole.ComCommentMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationCommentService.DeleteCommunicationCommentAttachmentAsync(authenticate, communicationId, fileSrc);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download comment Attachment
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{communicationId:long}/comment/downloadFile")]
        public async Task<object> DownloadCommentFileAsync(long communicationId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.TransmittalMng,
            //    SCMRole.TransmittalObs
            //};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _communicationCommentService.DownloadCommentFileAsync(authenticate, communicationId, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, "application/octet-stream");
        }
       
        /// <summary>
        /// Add Team Comment 
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{communicationId:long}/comment/TeamComment")]
        public async Task<object> AddTeamCommentAsync(long communicationId, [FromBody] AddCommunicationTeamCommentDto model)
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
            //    SCMRole.ComCommentLimitedObs,
            //    SCMRole.ComCommentObs,
            //    SCMRole.ComCommentReg,
            //    SCMRole.ComCommentReply,
            //    SCMRole.ComCommentMng,
            //};
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationTeamCommentService.AddCommentAsync(authenticate, communicationId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Team Comment 
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("{communicationId:long}/comment/TeamComment")]
        public async Task<object> GetTeamCommentAsync(long communicationId, [FromQuery] CommunicationTeamCommentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            // authenticate.Roles = new List<string> { SCMRole.RevisionMng };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationTeamCommentService.GetCommentAsync(authenticate, communicationId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get User Mention Of Comment 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("comment/userMention")]
        public async Task<object> GetUserMentionOfCommentAsync([FromQuery] UserQueryDto query)
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
        [Route("{communicationId:long}/comment/downloadAttachments")]
        public async Task<object> GenerateCommentAttachmentZipAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.TransmittalMng,
            //    SCMRole.TransmittalObs
            //};
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var ArchiveFileResult = await _communicationCommentService.GenerateCommentAttachmentZipAsync(authenticate, communicationId);
            if (ArchiveFileResult == null)
                return NotFound();

            return File(ArchiveFileResult.Stream, ArchiveFileResult.ContentType,ArchiveFileResult.FileName);
            //return File(ArchiveFileResult.ArchiveFile, ArchiveFileResult.ContentType, ArchiveFileResult.FileName);
        }
        [HttpGet]
        [Route("{communicationId:long}/comment/generatePdf")]
        public async Task<object> GenerateNCRPdfAsync(long communicationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.TransmittalMng,
            //    SCMRole.TransmittalObs
            //};
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var ArchiveFileResult = await _communicationCommentService.GenerateCommentPdfAsync(authenticate, communicationId);
            if (ArchiveFileResult == null)
                return NotFound();

            return File(ArchiveFileResult.ArchiveFile, "application/octet-stream");
            //return File(ArchiveFileResult.ArchiveFile, ArchiveFileResult.ContentType, ArchiveFileResult.FileName);
        }

        [HttpGet, Route("comment/commentPendingReply")]
        public async Task<object> GetPendingCommunicationListAsync([FromQuery] CommunicationQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> { SCMRole.RevisionMng, SCMRole.RevisionObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentCommunicationService.GetPendingReplyCommunicationListAsync(authenticate, query, CommunicationType.Comment);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Download comment and reply attachment
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="commentId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{communicationId:long}/commentCommentAndReply/{commentId:long}/downloadFile")]
        public async Task<object> DownloadCommentAndReplyFileAsync(long communicationId,long commentId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> {
            //    SCMRole.TransmittalMng,
            //    SCMRole.TransmittalObs
            //};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _communicationCommentService.DownloadCommentFileAsync(authenticate, communicationId, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }
    }
}