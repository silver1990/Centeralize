using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Document;
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
    [Route("api/SCMCustomerDocumentManagement/v1/communication")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "SCMCustomerPanelManagement")]
    public class CustomerCommunicationController : ControllerBase
    {

        private readonly IDocumentService _documentService;
        private readonly IContractDocumentGroupService _contractDocumentGroupService;
        private readonly IDocumentCommunicationService _documentCommunicationService;
        private readonly IUserService _userService;
        private readonly IBomProductService _bomProductService;
        private readonly ILogger<CustomerCommunicationController> _logger;

        public CustomerCommunicationController(
            IDocumentService documentService,
            ILogger<CustomerCommunicationController> logger,IHttpContextAccessor httpContextAccessor,
            IContractDocumentGroupService contractDocumentGroupService,
            IDocumentCommunicationService documentCommunicationService, IBomProductService bomProductService, IUserService userService)
        {
            _documentService = documentService;
            _logger = logger;
            _contractDocumentGroupService = contractDocumentGroupService;
            _documentCommunicationService = documentCommunicationService;
            _bomProductService = bomProductService;
            _userService = userService;
           
               
           
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
            authenticate.Roles = new List<string>();
            
            var accessability = await _userService.IsUserCustomerAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentService.GetLastTransmittalRevisionForCustomerAsync(authenticate, query, accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get document group list
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("DocumentGroup")]
        public async Task<object> GetDocumentGroupListAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _contractDocumentGroupService.GetDocumentGroupListForCustomerUserAsync(authenticate,accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

     
        /// <summary>
        /// Get Bom Product List For Document ContractCode
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("bomProduct")]
        public async Task<object> BomProducts()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();

            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _bomProductService.GetBomProductForDocumentByContractCodeForCustomerUserAsync(authenticate,accessability.Result);
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
            authenticate.Roles = new List<string>();
            var accessability = await _userService.IsUserCustomerOrSupperUserAccess(authenticate);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _documentCommunicationService.GetCurrentContractInfoForCustomerUserAsync(authenticate, accessability.Result);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
