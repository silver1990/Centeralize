using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.PurchaseRequest;
using Raybod.SCM.DataTransferObject.RFP;
using Raybod.SCM.DataTransferObject.RFP.RFPComment;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.ModuleApi.Areas.ProcurementManagement.Controllers
{
    [Route("api/procurementManagement/rfp")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class RFPController : ControllerBase
    {

        private readonly IPurchaseRequestService _purchaseRequestService;
        private readonly IRFPService _rfpService;
        private readonly ISupplierService _supplierService;
        private readonly ILogger<RFPController> _logger;

        public RFPController(
            IPurchaseRequestService purchaseRequestService,
            IRFPService rfpService,
            ISupplierService supplierService,
            ILogger<RFPController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _purchaseRequestService = purchaseRequestService;
            _rfpService = rfpService;
            _supplierService = supplierService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// get waiting pr list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("WaitingForRfpList")]
        [HttpGet]
        public async Task<object> GetWaitingPRListAsync([FromQuery] PurchaseRequestQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng, SCMRole.RFPObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _purchaseRequestService.GetWaitingPRForNewRFPListAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Waiting PR Badge 
        /// </summary>
        /// <returns></returns>
        [Route("badgeCounter")]
        [HttpGet]
        public async Task<object> GetWaitingPRBadgeCountAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng, SCMRole.RFPObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetRFPListBadgeCountAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Get Waiting PR item For Add new RFP 
        /// </summary>
        /// <param name="purchaseRequestId"></param>
        /// <returns></returns>
        [Route("Waiting/{purchaseRequestId:long}/PurchaseRequestDetail")]
        [HttpGet]
        public async Task<object> GetWaitingPRForAddRFPByPRIdAsync(long purchaseRequestId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng, SCMRole.RFPObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, purchaseRequestId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _purchaseRequestService.GetWaitingPRForAddRFPByPRIdAsync(authenticate, purchaseRequestId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Get Supplier Of Support This Products 
        /// </summary>
        /// <param name="productGroupId"></param>
        /// <returns></returns>
        [Route("Supplier/{productGroupId:int}")]
        [HttpGet]
        public async Task<object> GetSupplierOfSupportThisProductGroupsAsync(int productGroupId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng, SCMRole.RFPObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, productGroupId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _supplierService.GetSupplierOfSupportThisProductGroupsAsync(authenticate, productGroupId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// get rfp details
        /// </summary>
        /// <param name="rfpId"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/RFPDetail")]
        [HttpGet]
        public async Task<object> GetRFPDetail(long rfpId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng, SCMRole.RFPObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetRFPDetailsByIdAsync(authenticate, rfpId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add new rfp
        /// </summary>
        /// <param name="productGroupId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{productGroupId:int}")]
        public async Task<object> AddRFPAsync(int productGroupId, [FromBody] AddRFPDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            
            authenticate.Roles = new List<string> { SCMRole.RFPMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.AddRFPAsync(authenticate, productGroupId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Edit rfp items 
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{rfpId:long}/editRFPItems")]
        public async Task<object> EditRFPItemsAsync(long rfpId, [FromBody] List<AddRFPItemDto> model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            
            authenticate.Roles = new List<string> { SCMRole.RFPMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.RFPItemsEditAsync(authenticate, rfpId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
       
        

        
       

        /// <summary>
        /// Get InProgress RFP list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("InProgress")]
        public async Task<object> GetInProgressAsync([FromQuery] RFPQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng, SCMRole.RFPObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetInProgressRFPAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Get rfp list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("list")]
        public async Task<object> GetCompeletedsAsync([FromQuery] RFPQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng, SCMRole.RFPObs };



            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetRFPAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }



        /// <summary>
        /// get rfp details
        /// </summary>
        /// <param name="rfpId"></param>
        /// <returns></returns>
        [Route("{rfpId:long}")]
        [HttpGet]
        public async Task<object> GetRFPByIdAsync(long rfpId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng, SCMRole.RFPObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, rfpId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetRFPByIdAsync(authenticate, rfpId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        
    }
}
