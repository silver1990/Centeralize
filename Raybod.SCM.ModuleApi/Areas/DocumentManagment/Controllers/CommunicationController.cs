using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Raybod.SCM.DataTransferObject.Customer;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.Document.Communication;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Utility.Filters;


namespace Raybod.SCM.ModuleApi.Areas.DocumentManagment.Controllers
{
    [Route("api/Document/v1/communication")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "documentManagement")]
    public class CommunicationController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentCommunicationService _documentCommunicationService;
        private readonly IContractDocumentGroupService _contractDocumentGroupService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<CommunicationController> _logger;

        public CommunicationController(
            IDocumentService documentService,
            IDocumentCommunicationService documentCommunicationService,
            IContractDocumentGroupService contractDocumentGroupService,
            ICustomerService customerService,
            ILogger<CommunicationController> logger,IHttpContextAccessor httpContextAccessor
            )
        {
            _documentService = documentService;
            _documentCommunicationService = documentCommunicationService;
            _contractDocumentGroupService = contractDocumentGroupService;
            _customerService = customerService;
            _logger = logger;
           
            
                
           
           
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
                SCMRole.ComCommentLimitedObs,
                SCMRole.ComCommentLimitedGlbObs,
                SCMRole.ComCommentObs,
                SCMRole.ComCommentReg,
                SCMRole.ComCommentReply,
                SCMRole.ComCommentMng,
                SCMRole.NCRLimitedObs,
                SCMRole.NCRLimitedGlbObs,
                SCMRole.NCRObs,
                SCMRole.NCRReg,
                SCMRole.NCRReply,
                SCMRole.NCRMng,
                SCMRole.TQLimitedObs,
                SCMRole.TQLimitedGlbObs,
                SCMRole.TQObs,
                SCMRole.TQReg,
                SCMRole.TQReply,
                SCMRole.TQMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _contractDocumentGroupService.GetDocumentGroupListAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get customer List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("customer")]
        public async Task<object> GetCustomerMiniInfoWithoutPageingAsync([FromQuery] CustomerQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.ComCommentLimitedObs,
                SCMRole.ComCommentLimitedGlbObs,
                SCMRole.ComCommentObs,
                SCMRole.ComCommentReg,
                SCMRole.ComCommentReply,
                SCMRole.ComCommentMng,
                SCMRole.NCRLimitedObs,
                SCMRole.NCRLimitedGlbObs,
                SCMRole.NCRObs,
                SCMRole.NCRReg,
                SCMRole.NCRReply,
                SCMRole.NCRMng,
                SCMRole.TQLimitedObs,
                SCMRole.TQLimitedGlbObs,
                SCMRole.TQObs,
                SCMRole.TQReg,
                SCMRole.TQReply,
                SCMRole.TQMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _customerService.GetCustomerMiniInfoWithoutPageingAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        ///  get Company List
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("Company")]
        public async Task<object> GetAllCompanyListAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.ComCommentLimitedObs,
                SCMRole.ComCommentLimitedGlbObs,
                SCMRole.ComCommentObs,
                SCMRole.ComCommentReg,
                SCMRole.ComCommentReply,
                SCMRole.ComCommentMng,
                SCMRole.NCRLimitedObs,
                SCMRole.NCRLimitedGlbObs,
                SCMRole.NCRObs,
                SCMRole.NCRReg,
                SCMRole.NCRReply,
                SCMRole.NCRMng,
                SCMRole.TQLimitedObs,
                SCMRole.TQLimitedGlbObs,
                SCMRole.TQObs,
                SCMRole.TQReg,
                SCMRole.TQReply,
                SCMRole.TQMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentCommunicationService.GetAllCompanyListAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get last transmittal revision List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("LastTransmittalRevision")]
        public async Task<object> GetLastTransmittalRevisionAsync([FromQuery] DocumentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.ComCommentLimitedObs,
                SCMRole.ComCommentObs,
                SCMRole.ComCommentReg,
                SCMRole.ComCommentReply,
                SCMRole.ComCommentMng,
                SCMRole.NCRLimitedObs,
                SCMRole.NCRObs,
                SCMRole.NCRReg,
                SCMRole.NCRReply,
                SCMRole.NCRMng,
                SCMRole.TQLimitedObs,
                SCMRole.TQObs,
                SCMRole.TQReg,
                SCMRole.TQReply,
                SCMRole.TQMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentService.GetLastTransmittalRevisionAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Current Contract Info 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("CurrentContractInfo")]
        public async Task<object> GetCurrentContractInfoAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.ComCommentLimitedObs,
                SCMRole.ComCommentLimitedGlbObs,
                SCMRole.ComCommentObs,
                SCMRole.ComCommentReg,
                SCMRole.ComCommentReply,
                SCMRole.ComCommentMng,
                SCMRole.NCRLimitedObs,
                SCMRole.NCRLimitedGlbObs,
                SCMRole.NCRObs,
                SCMRole.NCRReg,
                SCMRole.NCRReply,
                SCMRole.NCRMng,
                SCMRole.TQLimitedObs,
                SCMRole.TQLimitedGlbObs,
                SCMRole.TQObs,
                SCMRole.TQReg,
                SCMRole.TQReply,
                SCMRole.TQMng,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentCommunicationService.GetCurrentContractInfoAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get pending reply Communication List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        //[HttpGet, Route("pendingReply")]
        //public async Task<object> GetPendingCommunicationListAsync([FromQuery] CommunicationQueryDto query)
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    //authenticate.Roles = new List<string> { SCMRole.RevisionMng, SCMRole.RevisionObs };

        //    var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
        //    _logger.LogWarning(logInformation.InformationText, logInformation.Args);
        //    var serviceResult = await _documentCommunicationService.GetPendingReplyCommunicationListAsync(authenticate, query);
        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}

        /// <summary>
        /// Get pending reply Communication List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("pendingReplyBadge")]
        public async Task<object> GetPendingReplyCommunicationBadgeAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            //authenticate.Roles = new List<string> { SCMRole.RevisionMng, SCMRole.RevisionObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentCommunicationService.GetPendingReplyCommunicationBadgeAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}