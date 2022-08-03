using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Utility.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raybod.SCM.DataTransferObject.PRContract;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.DataTransferObject.RFP;
using Raybod.SCM.DataTransferObject.Product;
using Raybod.SCM.DataTransferObject.Supplier;
using Raybod.SCM.DataTransferObject.ProductGroup;
using Raybod.SCM.DataTransferObject;
using Microsoft.Extensions.Logging;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.PurchaseManagement.Controllers
{
    [Route("api/procurementManagement/prContract")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class PRContractController : ControllerBase
    {
        private readonly IPRContractService _prContractService;
        private readonly IRFPService _rfpService;
        private readonly IProductService _productService;
        private readonly IProductGroupService _productGroupService;
        private readonly ISupplierService _supplierService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PRContractController> _logger;

        public PRContractController(
            IPRContractService prContractService,
            IRFPService rfpService,
            IProductService productService,
            IProductGroupService productGroupService,
            ISupplierService supplierService,
            IPaymentService paymentService,
            ILogger<PRContractController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _supplierService = supplierService;
            _productGroupService = productGroupService;
            _productService = productService;
            _prContractService = prContractService;
            _rfpService = rfpService;
            _paymentService = paymentService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// Get Waiting RFP For Create PRContract Badge
        /// </summary>
        /// <returns></returns>
        [Route("WaitingRFPBadge")]
        [HttpGet]
        public async Task<object> GetWaitingRFPItemForCreatePRContractBadgeAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetWaitingRFPItemForCreatePRContractBadgeAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        ///  get pr contract waiting confirm badge
        /// </summary>
        /// <returns></returns>
        [Route("PendingPrContractBadge")]
        [HttpGet]

        public async Task<object> GetPendingPRContractBadgeAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _prContractService.GetPendingPRContractBadgeAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get pr contract waiting confirm list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("PendingPrContract")]
        public async Task<object> GetConfirms([FromQuery] PRContractQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };



            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _prContractService.GetPendingForConfirmPrContractstAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        


       
        /// <summary>
        /// Get Product Group List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("ProductGroup")]
        [HttpGet]
        public async Task<object> GetProductGroupAsync([FromQuery] string query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var queryy = new ProductGroupQuery();
            queryy.SearchText = query;

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productGroupService.GetProductGroupAsync(authenticate, queryy);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Equipment Product list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("EquipmentProduct")]
        [HttpGet]
        public async Task<object> GetEquipmentProductAsync([FromQuery] ProductQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };
            query.ProductType = Domain.Enum.ProductType.Equipment;

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productService.GetProductMiniInfoAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        
        /// <summary>
        /// get supplier list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("supplierList")]
        [HttpGet]
        public async Task<object> GetSuppliersListAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _supplierService.GetSuppliersListAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// get supplier list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("supplier")]
        [HttpGet]
        public async Task<object> GetWinnerRFPSuppliersAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _supplierService.GetWinnerRFPSuppliersAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        

        #region waiting rfp and supplier

        /// <summary>
        /// Get Waiting RFP list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("WaitingRFP")]
        public async Task<object> GetRFPForCreatePRContractAsync([FromQuery] RFPQueryDto query) // todo remove
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetRFPForCreatePRContractAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Waiting RFPItem for create prContract
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("WaitingRFPItems")]
        [HttpGet]
        public async Task<object> GetWaitingRFPItemsForCreatePrContractAsync(long? prContractId,[FromQuery] RFPItemQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _rfpService.GetWaitingRFPItemsForCreatePrContractAsync(authenticate, query,prContractId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Waiting RFPItem By RFPId 
        /// </summary>
        /// <param name="rfpId"></param>
        /// <returns></returns>
        [Route("WaitingRFP/{rfpId:long}")]
        [HttpGet]
        public async Task<object> GetRFPItemsByRFPIdForCreatePrContractAsync(long rfpId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _rfpService.GetRFPItemsByRFPIdForCreatePrContractAsync(authenticate, rfpId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get RFPItem OfThisPRContract by prContractId 
        /// </summary>
        /// <param name="prContractId"></param>
        /// <returns></returns>
        [Route("{prContractId:long}/rfpItems")]
        [HttpGet]
        public async Task<object> GetRFPItemOfThisPRContractbyprContractIdAsync(long prContractId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _prContractService.GetRFPItemOfThisPRContractbyprContractIdAsync(authenticate, prContractId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Winner rfp Supplier list By RFPId 
        /// </summary>
        /// <param name="rfpId"></param>
        /// <returns></returns>
        [Route("rfpSupplier/{rfpId:long}")]
        [HttpGet]
        public async Task<object> GetWinnerSupplierRFPByRFPIdAsync(long rfpId) //to do remove
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetWinnerSupplierRFPByRFPIdAsync(authenticate, rfpId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        #endregion

       
        
        /// <summary>
        /// get prContract list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetPRContractsAsync([FromQuery] PRContractQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _prContractService.GetPRContractsAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

       
        

        /// <summary>
        /// get pr confirm user  list 
        /// </summary>
        /// <param name="productGroupId"></param>
        /// <returns></returns>
        [HttpGet, Route("workFlowRegister/{productGroupId:int}/users")]
        public async Task<object> GetConfirmUserAsync(int productGroupId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, productGroupId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _prContractService.GetConfirmationUserListAsync(authenticate, productGroupId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        
    }
}