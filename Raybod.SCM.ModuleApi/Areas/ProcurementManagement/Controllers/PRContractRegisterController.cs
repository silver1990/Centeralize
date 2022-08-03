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
    public class PRContractRegisterController : ControllerBase
    {
        private readonly IPRContractService _prContractService;
        private readonly IRFPService _rfpService;
        private readonly IProductService _productService;
        private readonly IProductGroupService _productGroupService;
        private readonly ISupplierService _supplierService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PRContractRegisterController> _logger;

        public PRContractRegisterController(
           IPRContractService prContractService,
           IRFPService rfpService,

           IProductService productService,
           IProductGroupService productGroupService,
           ISupplierService supplierService,
           IPaymentService paymentService,
           ILogger<PRContractRegisterController> logger,IHttpContextAccessor httpContextAccessor)
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
        /// add new prContract
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> AddPRContractAsync([FromBody] AddPRContractDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage>
                                {new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid)})
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _prContractService.AddPRContractAsync(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// edit prContract
        /// </summary>
        /// <param name="model"></param>
        /// <param name="prContractId"></param>
        /// <returns></returns>
        [HttpPost, Route("{prContractId:long}/editPrContract")]
        public async Task<object> EditPRContractAsync(long prContractId, [FromBody] AddPRContractDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage>
                                {new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid)})
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _prContractService.EditPRContractAsync(authenticate, prContractId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Cancel rfp
        /// </summary>
        /// <param name="prContractId"></param>
        /// <returns></returns>
        [HttpPut, Route("{prContractId:long}/cancelPrContract")]
        public async Task<object> CancelRFPAsync(long prContractId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _prContractService.CancelPrContractAsync(authenticate, prContractId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        [HttpPost, Route("workflowConfirm/{prContractId:long}/ConfirmationTask")]
        public async Task<object> SetUserConfirmOwnpurchaseRequestTaskAsync(long prContractId, [FromBody] AddPrContractConfirmationAnswerDto model)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.PrContractMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _prContractService.SetUserConfirmOwnPrContractTaskAsync(authenticate, prContractId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
