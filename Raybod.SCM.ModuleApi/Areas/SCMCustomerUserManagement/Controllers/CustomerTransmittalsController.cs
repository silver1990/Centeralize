using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Transmittal;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Utility.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.SCMCustomerUserManagement.Controllers
{
    [Route("api/SCMCustomerDocumentManagement/v1/CustomerTransmittals")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "SCMCustomerPanelManagement")]
    public class CustomerTransmittalsController : ControllerBase
    {
        private readonly ITransmittalService _transmittalService;
        private readonly IDocumentRevisionService _documentRevisionService;
        private readonly IRevisionConfirmationService _revisionConfirmationService;
        private readonly IUserService _userService;
        private readonly IContractDocumentGroupService _contractDocumentGroupService;
        private readonly ILogger<CustomerTransmittalsController> _logger;

        public CustomerTransmittalsController(
            ITransmittalService transmittalService,
            IDocumentRevisionService documentRevisionService,
            IRevisionConfirmationService revisionConfirmationService,
            IContractDocumentGroupService contractDocumentGroupService,
            ILogger<CustomerTransmittalsController> logger,IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _transmittalService = transmittalService;
            _revisionConfirmationService = revisionConfirmationService;
            _documentRevisionService = documentRevisionService;
            _contractDocumentGroupService = contractDocumentGroupService;
            _logger = logger;
            _userService = userService;
           
               
           
        }

        /// <summary>
        /// Get Transmittal List 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetTransmittalListAsync([FromQuery] TransmittalQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.GetTransmittalLisForCustomerUsertAsync(authenticate, query,true, accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Transmittaled Revision List For Export To Excel
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("forExportToExcel")]
        public async Task<object> GetTransmittaledRevisionListForExportToExcelAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.GetTransmittaledRevisionListForExportToExcelCustomerUserlAsync(authenticate,true,accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Transmittal Details by transmittalId
        /// </summary>
        /// <param name="transmittalId"></param>
        /// <returns></returns>
        [HttpGet, Route("{transmittalId:long}")]
        public async Task<object> GetTransmittalDetailsAsync(long transmittalId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.GetTransmittalDetailsForCustomerUserAsync(authenticate, transmittalId, accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download Revision Final Attachment
        /// </summary>
        /// <param name="revisionId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DocumentRevisoin/{revisionId:long}/downloadFinalFile")]
        public async Task<object> DownloadRevisionFinalAttachmentAsync(long revisionId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _documentRevisionService.DownloadRevisionFileForCustomerUserAsync(authenticate, revisionId, fileSrc, RevisionAttachmentType.Final, accessability.Result);
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
        [HttpGet, Route("{transmittalId:long}/downloadFile")]

        public async Task<object> DownloadTransmitalFileAsync(long transmittalId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _transmittalService.DownloadTransmitalFileForCustomerUserAsync(authenticate, transmittalId,accessability.Result,RevisionAttachmentType.Final);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, "application/octet-stream");
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
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionConfirmationService.GetReportConfiemRevisionByRevIdForCustomerUserAsync(authenticate, revisionId,accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
