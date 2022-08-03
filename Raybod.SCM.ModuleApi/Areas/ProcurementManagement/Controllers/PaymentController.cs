using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Payment;
using Raybod.SCM.DataTransferObject.PendingForPayment;
using Raybod.SCM.DataTransferObject.Supplier;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.Financial.Controllers
{
    [Route("api/procurementManagement/payment")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class FinancialPaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ISupplierService _supplierService;
        private readonly ILogger<FinancialPaymentController> _logger;

        public FinancialPaymentController(
            IPaymentService paymentService,
            ISupplierService supplierService,
            ILogger<FinancialPaymentController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _paymentService = paymentService;
            _supplierService = supplierService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// get supplier list async
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("supplier")]
        [HttpGet]
        public async Task<object> GetSupplierAsync([FromQuery] SupplierQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PaymentMng, SCMRole.PaymentObs,SCMRole.PaymentConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.GetSuppliersAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Not Settled PendingForPayment list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("pendingForPaymentList")]
        public async Task<object> GetNotSettledPendingForPaymentAsync([FromQuery] PendingForPaymentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PaymentMng, SCMRole.PaymentObs, SCMRole.PaymentConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.GetNotSettledPendingForPaymentAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get PendingForPayment BadgeCount
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("PendingForPayment/NotSettledBadgeCount")]
        public async Task<object> GetPendingForPaymentBadgeCountAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PaymentMng, SCMRole.PaymentObs, SCMRole.PaymentConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.GetPendingForPaymentBadgeCountAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get PendingForPayment item details
        /// </summary>
        /// <param name="pendingForPaymentId"></param>
        /// <returns></returns>
        [HttpGet, Route("pendingForPayment/{pendingForPaymentId:long}")]
        public async Task<object> GetPendingOfPaymentForPayByPendingOfPaymentIdAsync(long pendingForPaymentId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PaymentMng, SCMRole.PaymentObs, SCMRole.PaymentConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, pendingForPaymentId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.GetPendingOfPaymentForPayByPendingOfPaymentIdAsync(authenticate, pendingForPaymentId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get PendingOfPayment For Pay By SupplierId
        /// </summary>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        [HttpGet, Route("pendingForPayment/supplier/{supplierId:int}")]
        public async Task<object> GetPendingOfPaymentForPayBySupplierIdAsync(int supplierId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PaymentMng, SCMRole.PaymentObs, SCMRole.PaymentConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, supplierId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.GetPendingOfPaymentForPayBySupplierIdAsync(authenticate, supplierId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add payment
        /// </summary>
        /// <param name="supplierId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{supplierId:int}")]
        public async Task<object> AddPaymentByPendingForPaymentAsync(int supplierId, [FromBody]  AddPaymentDto model)
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

            
            authenticate.Roles = new List<string> { SCMRole.PaymentMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.AddPaymentByPendingForPaymentAsync(authenticate, supplierId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get payment list async
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetPaymentAsync([FromQuery] PaymentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PaymentMng, SCMRole.PaymentObs, SCMRole.PaymentConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.GetPaymentAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get payment item details
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        [HttpGet, Route("{paymentId:long}")]
        public async Task<object> GetPaymentByIdAsync(long paymentId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PaymentMng, SCMRole.PaymentObs, SCMRole.PaymentConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, paymentId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.GetPaymentInfoByIdAsync(authenticate, paymentId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// download payment attachment 
        /// </summary>
        /// <param name="paymentId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet, Route("{paymentId:long}/attachment/Download")]
        public async Task<object> DownloadPaymentAttachmentAsync(long paymentId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PaymentObs, SCMRole.PaymentMng, SCMRole.PaymentConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _paymentService.DownloadPaymentAttachmentAsync(authenticate, paymentId, fileSrc);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, "application/octet-stream");

        }
        /// <summary>
        /// Cancel PendingForPayment 
        /// </summary>
        /// <returns></returns>
        [HttpPut, Route("cancelPendingForPayment/{pendingForPaymentId:long}")]
        public async Task<object> CancelPendingForPaymentAsync(long pendingForPaymentId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PaymentMng};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.CancelPendingForPaymentByPendingForPaymentIdAsync(authenticate,pendingForPaymentId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get payment confirm user  list 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("workFlowConfirmationUsers")]
        public async Task<object> GetConfirmUserAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PaymentMng, SCMRole.PaymentObs, SCMRole.PaymentConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.GetConfirmationUserListAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get  Pending Confirm list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("pendingForConfirmList")]
        public async Task<object> GetNotSettledPendingForConfirmAsync([FromQuery] PaymentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PaymentMng, SCMRole.PaymentObs, SCMRole.PaymentConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.GetPendingForConfirmPaymentAsync(authenticate,query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Get Payment Pending Confiem  Item by PaymentId
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        [HttpGet, Route("workflowConfirm/{paymentId:long}/pendingConfirm")]
        public async Task<object> GetPendingPaymentConfiemByPurchaseRequestIdAsync(long paymentId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.PaymentMng,
                SCMRole.PaymentObs,SCMRole.PaymentConfirm
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _paymentService.GetPendingConfirmPaymentByPaymentIdAsync(authenticate, paymentId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpPost, Route("workflowConfirm/{paymentId:long}/ConfirmationTask")]
        public async Task<object> SetUserConfirmOwnPaymentTaskAsync(long paymentId, [FromBody] AddPaymentConfirmationAnswerDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

           
            authenticate.Roles = new List<string> { SCMRole.PaymentConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _paymentService.SetUserConfirmOwnPurchaseRequestTaskAsync(authenticate, paymentId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet,Route("paymentList")]
        public async Task<object> GetPaymentListAsync([FromQuery] PaymentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PaymentMng, SCMRole.PaymentObs, SCMRole.PaymentConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.GetPaymentListAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }


}