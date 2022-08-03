using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.Email;
using Raybod.SCM.DataTransferObject.Transmittal;
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
    [Route("api/Document/v1/Transmittal")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "documentManagement")]
    public class TransmittalController : ControllerBase
    {
        private readonly ITransmittalService _transmittalService;
        private readonly IDocumentRevisionService _documentRevisionService;
        private readonly IRevisionConfirmationService _revisionConfirmationService;
        private readonly IContractDocumentGroupService _contractDocumentGroupService;
        private readonly ILogger<TransmittalController> _logger;


        public TransmittalController(
            ITransmittalService transmittalService,
            IDocumentRevisionService documentRevisionService,
            IRevisionConfirmationService revisionConfirmationService,
            IContractDocumentGroupService contractDocumentGroupService,
            ILogger<TransmittalController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _transmittalService = transmittalService;
            _revisionConfirmationService = revisionConfirmationService;
            _documentRevisionService = documentRevisionService;
            _contractDocumentGroupService = contractDocumentGroupService;
            _logger=logger;
           
               
           
        }

        /// <summary>
        /// get document group list
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("DocumentGroup")]
        public async Task<object> GetDocumentGroupListAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.TransmittalMng,
                SCMRole.TransmittalObs,
                SCMRole.TransmittalLimitedObs,
                SCMRole.TransmittalLimitedGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _contractDocumentGroupService.GetDocumentGroupListAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        ///  get Transmittal Company List
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("Company")]
        public async Task<object> GetTransmittalCompanyListAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.TransmittalMng,
                SCMRole.TransmittalObs,
                SCMRole.TransmittalLimitedObs,
                SCMRole.TransmittalLimitedGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.GetTransmittalCompanyListAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get company users by companyId
        /// for internal users set companyId equal 0
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="transmittalType"></param>
        /// <returns></returns>
        [HttpGet, Route("CompanyUsers/{companyId:int}")]
        public async Task<object> GetTransmittalCompanyUserListAsync(int companyId, TransmittalType transmittalType)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.TransmittalMng,
                SCMRole.TransmittalObs,
                SCMRole.TransmittalLimitedObs,
                SCMRole.TransmittalLimitedGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            switch (transmittalType)
            {
                case TransmittalType.Internal:
                    return (await _transmittalService.GetTransmittalInternalUserAsync(authenticate)).ToWebApiResultVCore(authenticate.language);
                case TransmittalType.Customer:
                    return (await _transmittalService.GetTransmittalCustomerUserAsync(authenticate, companyId)).ToWebApiResultVCore(authenticate.language);
                case TransmittalType.Supplier:
                    return (await _transmittalService.GetTransmittalSupplierUserAsync(authenticate, companyId)).ToWebApiResultVCore(authenticate.language);
                case TransmittalType.Consultant:
                    return (await _transmittalService.GetTransmittalConsultantUserAsync(authenticate, companyId)).ToWebApiResultVCore(authenticate.language);
                default:
                    return BadRequest();
            }

        }

        /// <summary>
        /// get revision List for add transmittal filter by documentGroupId
        /// </summary>
        /// <param name="documentGroupId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentGroup/{documentGroupId:int}/Revisions")]
        public async Task<object> GetRevisionForAddTransmittalAsync(int documentGroupId, [FromQuery] DocRevisionQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.TransmittalMng,
                SCMRole.TransmittalObs,
                SCMRole.TransmittalLimitedObs,
                SCMRole.TransmittalLimitedGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.GetRevisionForAddTransmittalAsync(authenticate, documentGroupId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add transmittal by revisions
        /// </summary>
        /// <param name="documentGroupId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("DocumentGroup/{documentGroupId:int}/addTransmittal")]
        public async Task<object> AddTransmittalAsync(int documentGroupId, [FromBody] AddTransmittalDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.TransmittalMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _transmittalService.AddTransmittalAsync(authenticate, documentGroupId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        ///  get Pending revisoin List grouped by documentGroup for create transmittal
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("PendingRevision")]
        public async Task<object> GetPendingRevisionAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.TransmittalMng,
                SCMRole.TransmittalObs,
                SCMRole.TransmittalLimitedObs,
                SCMRole.TransmittalLimitedGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.GetPendingRevisionGroupByDocumentGroupIdAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        

        /// <summary>
        /// get Pending Get Pending for Transmittal Details by documentGroupId
        /// </summary>
        /// <param name="documentGroupId"></param>
        /// <returns></returns>
        [HttpGet, Route("PendingRevision/{documentGroupId:int}")]
        public async Task<object> GetPendingRevisionAsync(int documentGroupId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.TransmittalMng,
                SCMRole.TransmittalObs,
                SCMRole.TransmittalLimitedObs,
                SCMRole.TransmittalLimitedGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.GetPendingTransmittalDetailsAsync(authenticate, documentGroupId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add transmittal by pending revision
        /// </summary>
        /// <param name="documentGroupId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{documentGroupId:int}")]
        public async Task<object> AddTransmittalByPendingRevisionAsync(int documentGroupId, [FromBody] AddTransmittalDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.TransmittalMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.AddTransmittalByPendingRevisionAsync(authenticate, documentGroupId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
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
            authenticate.Roles = new List<string> {
                SCMRole.TransmittalMng,
                SCMRole.TransmittalObs,
                SCMRole.TransmittalLimitedObs,
                SCMRole.TransmittalLimitedGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.GetTransmittalListAsync(authenticate, query,null);
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
            authenticate.Roles = new List<string> {
                SCMRole.TransmittalMng,
                SCMRole.TransmittalObs,
                SCMRole.TransmittalLimitedObs,
                SCMRole.TransmittalLimitedGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.GetTransmittalDetailsAsync(authenticate, transmittalId);
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
                SCMRole.TransmittalMng,
                SCMRole.TransmittalObs,
                SCMRole.TransmittalLimitedObs,
                SCMRole.TransmittalLimitedGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionConfirmationService.GetReportConfiemRevisionByRevIdAsync(authenticate, revisionId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
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
            authenticate.Roles = new List<string> {
                SCMRole.TransmittalMng,
                SCMRole.TransmittalObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _transmittalService.DownloadTransmitalFileAsync(authenticate, transmittalId,RevisionAttachmentType.Final);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, "application/octet-stream");
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
            authenticate.Roles = new List<string> {
                SCMRole.TransmittalMng,
                SCMRole.TransmittalObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _documentRevisionService.DownloadRevisionFileAsync(authenticate, revisionId,fileSrc,  RevisionAttachmentType.Final);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, "application/octet-stream");
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
            authenticate.Roles = new List<string> {
                SCMRole.TransmittalMng,
                SCMRole.TransmittalObs,
                SCMRole.TransmittalLimitedObs,
                SCMRole.TransmittalLimitedGlbObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.GetTransmittaledRevisionListForExportToExcelAsync(authenticate,null);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get transmittal email content
        /// </summary>
        /// <param name="transsmittalId"></param>
        /// <returns></returns>
        [HttpGet, Route("GetTransmittalEmailContent/{transsmittalId:long}")]
        public async Task<object> GetTransmittalEmailContent(long transsmittalId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TransmittalMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.GetTransmittalEmailContent(authenticate,transsmittalId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// send transmittal email 
        /// </summary>
        /// <param name="transsmittalId"></param>
        /// <returns></returns>
        [HttpPost, Route("SendTransmittalEmail/{transsmittalId:long}")]
        public async Task<object> SendTransmittalEmail(long transsmittalId,[FromBody]TransmittalEmailContentDto emailContent)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.TransmittalMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _transmittalService.SendTransmittalEmail(authenticate, transsmittalId,emailContent);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        //[HttpGet]
        //[Route("tetstttt")]
        //public async Task<object> test()
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> {
        //        SCMRole.TransmittalMng,
        //        SCMRole.TransmittalObs
        //    };

        //    var streamResult = _transmittalService.test();
        //    if (streamResult == null)
        //        return NotFound();

        //    return File(streamResult.Result.ArchiveFile, "application/octet-stream");
        //}
    }
}