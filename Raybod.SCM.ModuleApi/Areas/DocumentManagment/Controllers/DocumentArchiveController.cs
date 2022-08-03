using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;


namespace Raybod.SCM.ModuleApi.Areas.DocumentManagment.Controllers
{
    [Route("api/Document/v1/DocumentArchive")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "documentManagement")]
    public class DocumentArchiveController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentRevisionService _documentRevisionService;
        private readonly ITransmittalService _transmittalService;
        private readonly IRevisionConfirmationService _confirmationService;
        private readonly IDocumentCommunicationService _communicationService;
        private readonly ILogger<DocumentArchiveController> _logger;

        public DocumentArchiveController(
         IDocumentService documentService,
         IDocumentRevisionService documentRevisionService,
         ITransmittalService transmittalService,
         IRevisionConfirmationService confirmationService,
         IDocumentCommunicationService communicationService,
        ILogger<DocumentArchiveController> logger,IHttpContextAccessor httpContextAccessor
         )
        {
            _logger = logger;
            _communicationService = communicationService;
            _transmittalService = transmittalService;
            _documentService = documentService;
            _documentRevisionService = documentRevisionService;
            _confirmationService = confirmationService;
           
            
                
           
           
        }


        /// <summary>
        /// Get Document Archive by documentId
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet, Route("{documentId:long}/ArchiveDocument")]
        public async Task<object> GetDocumentArchiveAsync(long documentId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.DocumentArchiveObs,
                SCMRole.DocumentArchiveLimitedObs,
                SCMRole.DocumentArchiveLimitedGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentService.GetDocumentArchiveAsync(authenticate, documentId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Report Confiemation Revision By revisionId
        /// </summary>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/{revisionId:long}/ReportConfirmationLog")]
        public async Task<object> GetReportConfiemRevisionByRevIdAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                  SCMRole.DocumentArchiveObs,
                SCMRole.DocumentArchiveLimitedObs,
                SCMRole.DocumentArchiveLimitedGlbObs,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _confirmationService.GetReportConfiemRevisionByRevIdAsync(authenticate, revisionId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Report Revision communication list
        /// </summary>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/{revisionId:long}/Comunications")]
        public async Task<object> GetAllRevisionCommunicationListAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                  SCMRole.DocumentArchiveObs,
                SCMRole.DocumentArchiveLimitedObs,
                SCMRole.DocumentArchiveLimitedGlbObs,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationService.GetAllRevisionCommunicationListAsync(authenticate, revisionId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get Revision Final Attachment
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet, Route("Document/{documentId:long}/DocumentRevision/{revisionId:long}/FinalAttachment")]
        public async Task<object> GetRevisionFinalAttachmentAsync(long documentId, long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.DocumentArchiveObs,
                SCMRole.DocumentArchiveLimitedObs,
                SCMRole.DocumentArchiveLimitedGlbObs,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentRevisionService.GetDocumentRevisionAttachmentAsync(authenticate, documentId, revisionId, RevisionAttachmentType.Final);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get Revision Native Attachment
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet, Route("Document/{documentId:long}/DocumentRevision/{revisionId:long}/NativeAttachment")]
        public async Task<object> GetRevisionNativeAttachmentAsync(long documentId, long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.DocumentArchiveObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentRevisionService.GetDocumentRevisionAttachmentAsync(authenticate, documentId, revisionId, RevisionAttachmentType.FinalNative);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download Revision Final Attachment
        /// </summary>
        /// <param name="revisionId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DocumentRevision/{revisionId:long}/downloadFinalFile")]
        public async Task<object> DownloadRevisionFinalAttachmentAsync(long revisionId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.DocumentArchiveObs,
                SCMRole.DocumentArchiveLimitedObs,
                SCMRole.DocumentArchiveLimitedGlbObs,

            };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _documentRevisionService.DownloadRevisionFileAsync(authenticate, revisionId, fileSrc, RevisionAttachmentType.Final);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, "application/octet-stream");
        }

        /// <summary>
        /// Download Revision Native Attachment
        /// </summary>
        /// <param name="revisionId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DocumentRevision/{revisionId:long}/downloadNativeFile")]
        public async Task<object> DownloadRevisionNativeAttachmentAsync(long revisionId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.DocumentArchiveObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _documentRevisionService.DownloadRevisionFileAsync(authenticate, revisionId, fileSrc, RevisionAttachmentType.FinalNative);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, "application/octet-stream");
        }


        /// <summary>
        /// get Revision transmittal list by revisionId
        /// </summary>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DocumentRevision/{revisionId:long}/TransmittalList")]
        public async Task<object> GetTransmittalListAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.DocumentArchiveObs,
                SCMRole.DocumentArchiveLimitedObs,
                SCMRole.DocumentArchiveLimitedGlbObs,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.GetTransmittalListByRevisionIdAsync(authenticate, revisionId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Download Transmital File
        /// </summary>
        /// <param name="transmittalId"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/Transmittal/{transmittalId:long}/downloadFile")]

        public async Task<object> DownloadTransmitalFileAsync(long transmittalId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.DocumentArchiveObs,
                SCMRole.DocumentArchiveLimitedObs,
                SCMRole.DocumentArchiveLimitedGlbObs,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _transmittalService.DownloadTransmitalFileAsync(authenticate, transmittalId);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.ArchiveFile, streamResult.ContentType, streamResult.FileName);
        }
        /// <summary>
        /// Download Revision Native Attachment
        /// </summary>
        /// <param name="revisionId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DocumentRevision/{revisionId:long}/downloadNativeAndFinalFile")]
        public async Task<object> DownloadRevisionFinalAndNativeAttachmentAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.DocumentArchiveObs,
                SCMRole.DocumentArchiveLimitedObs,
                SCMRole.DocumentArchiveLimitedGlbObs,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _documentRevisionService.DownloadRevisionFileAsync(authenticate, revisionId);
            
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, "application/octet-stream");
        }


    }
}