using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Document;
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
    public class DocumentRevisionController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentRevisionService _documentRevisionService;
        private readonly IRevisionConfirmationService _revisionConfirmationService;
        private readonly ILogger<DocumentRevisionController> _logger;

        public DocumentRevisionController(
            IDocumentService documentService,
            IDocumentRevisionService documentRevisionService,
            IRevisionConfirmationService revisionConfirmationService,
            ILogger<DocumentRevisionController> logger,IHttpContextAccessor httpContextAccessor
            )
        {
            _revisionConfirmationService = revisionConfirmationService;
            _documentService = documentService;
            _documentRevisionService = documentRevisionService;
            _logger=logger;
           
            
                
           
           
        }

        /// <summary>
        /// Get Revision Badge Count 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/badge")]
        public async Task<object> GetRevisionBadgeCountAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionActivityMng,
                SCMRole.RevisionConfirmMng,
                SCMRole.RevisionConfirmGlbMng,
                SCMRole.RevisionMng,
                SCMRole.RevisionObs,
                SCMRole.RevisionGlbObs,
                SCMRole.RevisionCreator
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentRevisionService.GetPendingRevisonBadgeCountAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Active Document List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/ActiveDocument")]
        public async Task<object> GetActiveForAddRevisionDocumentAsync([FromQuery] DocumentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RevisionMng, SCMRole.RevisionObs , SCMRole.RevisionCreator };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentService.GetActiveForAddRevisionDocumentAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Inprogress Revision list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/Inprogress")]
        public async Task<object> GetInprogressDocumentRevisionAsync([FromQuery] DocRevisionQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionObs,
                SCMRole.RevisionGlbObs,
                SCMRole.RevisionActivityMng,
                 SCMRole.RevisionCreator
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentRevisionService.GetInprogressDocumentRevisionAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add Revision 
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("Document/{documentId:long}/DocumentRevision")]
        public async Task<object> AddDocumentRevisionAsync(long documentId, [FromBody] AddDocumentRevisionDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.RevisionMng  , SCMRole.RevisionCreator};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentRevisionService.AddDocumentRevisionAsync(authenticate, documentId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
       
        /// <summary>
        /// Edit Revision 
        /// </summary>
        /// <param name="documentRevisionId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("Document/DocumentRevision/{documentRevisionId:long}")]
        public async Task<object> EditDocumentRevisionAsync(long documentRevisionId, [FromBody] EditRevisionDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.RevisionMng, SCMRole.RevisionCreator };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentRevisionService.EditDocumentRevisionAsync(authenticate, documentRevisionId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// DeActive Revision Item
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpPost, Route("{documentId:long}/DocumentRevision/{revisionId:long}/DeActiveRevision")]
        public async Task<object> DeActiveRevisionAsync(long documentId, long revisionId)
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

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentRevisionService.DeActiveRevisionAsync(authenticate, documentId, revisionId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Revision Details by revisionId
        /// </summary>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/{revisionId:long}")]
        public async Task<object> GetDocumentRevisionDetailsAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionObs,
                SCMRole.RevisionGlbObs,
                SCMRole.RevisionActivityMng,
                SCMRole.RevisionCreator
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentRevisionService.GetDocumentRevisionByIdAsync(authenticate, revisionId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add Revision Preparation Attachment
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpPost, Route("Document/{documentId:long}/DocumentRevision/{revisionId:long}/attachment")]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<object> AddDocumentRevisionAttachmentAsync(long documentId, long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionActivityMng,
                SCMRole.RevisionCreator
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var files = HttpContext.Request.Form.Files;
            var serviceResult = await _documentRevisionService.AddDocumentRevisionAttachmentAsync(authenticate, documentId, revisionId, files);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get Revision Preparation Attachment
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet, Route("Document/{documentId:long}/DocumentRevision/{revisionId:long}/PreparationAttachment")]
        public async Task<object> GetRevisionPreparationAttachmentAsync(long documentId, long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionObs,
                SCMRole.RevisionGlbObs,
                SCMRole.RevisionActivityMng,
                SCMRole.RevisionCreator
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentRevisionService.GetDocumentRevisionAttachmentAsync(authenticate, documentId, revisionId, RevisionAttachmentType.Preparation);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Delete Revision Preparation Attachment
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="revisionId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpDelete, Route("Document/{documentId:long}/DocumentRevision/{revisionId:long}/attachment")]
        public async Task<object> DeleteDocumentRevisionAttachmentAsync(long documentId, long revisionId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionCreator
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentRevisionService.DeleteDocumentRevisionAttachmentAsync(authenticate, documentId, revisionId, fileSrc);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// download Revision Preparation Attachment
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="revisionId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Document/{documentId:long}/DocumentRevision/{revisionId:long}/downloadFile")]
        public async Task<object> DownloadRevisionFileAsync(long documentId, long revisionId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionObs,
                SCMRole.RevisionGlbObs,
                SCMRole.RevisionActivityMng,
                SCMRole.RevisionCreator
            };
            //var acceptType = new List<RevisionAttachmentType> { RevisionAttachmentType.Final, RevisionAttachmentType.FinalNative, RevisionAttachmentType.Preparation };

            //if (!acceptType.Contains(attachType))
            //    return BadRequest();
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _documentRevisionService.DownloadRevisionFileAsync(authenticate, documentId, revisionId, fileSrc);
            if (streamResult == null)
                return NotFound();

            //return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
            return File(streamResult.Stream, "application/octet-stream");
        }

        /// <summary>
        /// get Rejected User Confirmation Revision 
        /// </summary>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DocumentRevision/{revisionId:long}/RejectedUser")]
        public async Task<object> GetRejectedUserConfirmationRevisionAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionObs,
                SCMRole.RevisionGlbObs,
                SCMRole.RevisionActivityMng,
                SCMRole.RevisionCreator
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionConfirmationService.GetRejectedUserInfoConfirmationRevisionAsync(authenticate, revisionId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// get all report revision confirmation workFlow
        /// </summary>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DocumentRevision/{revisionId:long}/ReportRevisionConfirmationWorkFlow")]
        public async Task<object> GetReportRevisionConfirmationWorkFlowByRevIdAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionConfirmationService.GetReportRevisionConfirmationWorkFlowByRevIdAsync(authenticate, revisionId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Import File Attachment
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="revisionId"></param>
        /// /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpPost, Route("Document/{documentId:long}/DocumentRevision/{revisionId:long}/importFile")]
        public async Task<object> ImportFileAsync(long documentId, long revisionId , string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
           
            var serviceResult = await _documentRevisionService.ImportFileFromSharing(authenticate, documentId, revisionId, fileSrc);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add Revision 
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("Document/{documentId:long}/AddRevisionFromList")]
        public async Task<object> AddDocumentRevisionFromListAsync(long documentId, [FromBody] AddDocumentRevisionDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language, ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.RevisionMng, SCMRole.RevisionCreator };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentRevisionService.AddDocumentRevisionFromListAsync(authenticate, documentId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

    }
}