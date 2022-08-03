using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Utility.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.ModuleApi.Areas.SCMCustomerUserManagement.Controllers
{
    [Route("api/SCMCustomerDocumentManagement/v1/DocumentArchive")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "SCMCustomerPanelManagement")]
    public class CustomerDocumentArchive : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentRevisionService _documentRevisionService;
        private readonly ITransmittalService _transmittalService;
        private readonly IRevisionConfirmationService _confirmationService;
        private readonly IDocumentCommunicationService _communicationService;
        private readonly IUserService _userService;
        private readonly ILogger<CustomerDocumentArchive> _logger;

        public CustomerDocumentArchive(
         IDocumentService documentService,
         IDocumentRevisionService documentRevisionService,
         ITransmittalService transmittalService,
         IRevisionConfirmationService confirmationService,
         IDocumentCommunicationService communicationService,
        ILogger<CustomerDocumentArchive> logger,IHttpContextAccessor httpContextAccessor,
        IUserService userService)
        {
            _logger = logger;
            _communicationService = communicationService;
            _transmittalService = transmittalService;
            _documentService = documentService;
            _documentRevisionService = documentRevisionService;
            _confirmationService = confirmationService;
            _userService = userService;
           
               
           
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
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentService.GetDocumentArchiveForCustomerUserAsync(authenticate, documentId,accessability.Result);
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
            authenticate.Roles = new List<string>();
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentRevisionService.GetDocumentRevisionAttachmentForCustomerUserAsync(authenticate, documentId, revisionId, RevisionAttachmentType.Final,accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
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
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.GetTransmittalListByRevisionIdForCustomerUserAsync(authenticate, revisionId, accessability.Result);
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
            authenticate.Roles = new List<string>();
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _documentRevisionService.DownloadRevisionFileForCustomerUserAsync(authenticate, revisionId, fileSrc, RevisionAttachmentType.Final,accessability.Result);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, "application/octet-stream");
        }

        /// <summary>
        /// Download Transmital File
        /// </summary>
        /// <param name="transmittalId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/Transmittal/{transmittalId:long}/downloadFile")]

        public async Task<object> DownloadTransmitalFileAsync(long transmittalId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _transmittalService.DownloadTransmitalFileForCustomerUserAsync(authenticate, transmittalId, accessability.Result);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.ArchiveFile, streamResult.ContentType, streamResult.FileName);
        }


        /// <summary>
        /// Get Report Revision Comment list
        /// </summary>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/{revisionId:long}/Comments")]
        public async Task<object> GetAllRevisionCommentListAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationService.GetAllRevisionCommentListForCustomerUserAsync(authenticate, revisionId,accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Report Revision TQ list
        /// </summary>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/{revisionId:long}/TQ")]
        public async Task<object> GetAllRevisionTQListAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationService.GetAllRevisionTQListForCustomerUserAsync(authenticate, revisionId, accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Report Revision NCR list
        /// </summary>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/{revisionId:long}/NCR")]
        public async Task<object> GetAllRevisionNCRListAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _communicationService.GetAllRevisionNCRListForCustomerUserAsync(authenticate, revisionId, accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download Revision Native Attachment
        /// </summary>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DocumentRevision/{revisionId:long}/downloadRevisionFile")]
        public async Task<object> DownloadRevisionFinalAndNativeAttachmentAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _documentRevisionService.DownloadRevisionFileForCustomerAsync(authenticate, revisionId,accessability.Result);

            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, "application/octet-stream");
        }
    }
}
