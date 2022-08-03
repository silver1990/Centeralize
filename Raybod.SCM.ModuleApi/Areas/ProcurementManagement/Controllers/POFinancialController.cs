using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.PendingForPayment;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.PurchaseManagement.Controllers
{
    [Route("api/procurementManagement/PO/{poId:long}/Financial")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class POFinancialController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IPaymentService _paymentService;
        private readonly IPOService _poService;
        private readonly ILogger<POFinancialController> _logger;

        public POFinancialController(
            IInvoiceService invoiceService,
            IPaymentService paymentService,
            ILogger<POFinancialController> logger,IHttpContextAccessor httpContextAccessor, IPOService pOService)
        {
            _invoiceService = invoiceService;
            _paymentService = paymentService;
            _logger = logger;
            _poService = pOService;
           
               
           
        }
       
        /// <summary>
        /// get pendingForPayment list
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpGet, Route("requestForPaymentList")]
        public async Task<object> GetPendingForPaymentByPOIdAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POFinancialMng, SCMRole.POFinancialObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.GetPendingForPaymentByPOIdAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

   

        /// <summary>
        /// Add PendingToPayment Base On TermsOfPayment Except Invoice
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="requestAmount"></param>
        /// <param name="paymentStep"></param>
        /// <returns></returns>
        [HttpPost, Route("pendingForPayment/AddByPOPaymentStep")]
        public async Task<object> AddPendingToPaymentBaseOnTermsOfPaymentExceptInvoiceAsync(long poId,[FromBody] AddPendingForPaymentDto model )
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POFinancialMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.AddPendingToPaymentBaseOnTermsOfPaymentExceptInvoiceAsync(authenticate, poId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get invoice by poid
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpGet, Route("invoice")]
        public async Task<object> GetInvoiceByPOIdAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POFinancialMng, SCMRole.POFinancialObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _invoiceService.GetInvoiceByPOIdAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// delete request for payment
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="pendingForPaymentId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{pendingForPaymentId:long}")]
        public async Task<object> DeletePendingForPaymentByPOIdAsync(long poId,long pendingForPaymentId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POFinancialMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.DeletePendingForPaymentByPOIdAsync(authenticate, poId, pendingForPaymentId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}