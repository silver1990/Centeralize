using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Invoice;
using Raybod.SCM.DataTransferObject.Product;
using Raybod.SCM.DataTransferObject.Supplier;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Authentication;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;

namespace Raybod.SCM.ModuleApi.Areas.Financial.Controllers
{
    [Route("api/procurementManagement/invoice")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class FinancialInvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IProductService _productService;
        private readonly IProductGroupService _productGroupService;
        private readonly ISupplierService _supplierService;
        private readonly ILogger<FinancialInvoiceController> _logger;


        public FinancialInvoiceController(
            IInvoiceService invoiceService,
            ISupplierService supplierService,
            IProductGroupService productGroupService,
            IProductService productService,
            ILogger<FinancialInvoiceController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _invoiceService = invoiceService;
            _productService = productService;
            _productGroupService = productGroupService;
            _supplierService = supplierService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// get product group list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("ProductGroup")]
        [HttpGet]
        public async Task<object> GetProductGroupAsync([FromQuery] string query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.InvoiceMng, SCMRole.InvoiceObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productGroupService.GetProductGroupAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get EquipmentProduct product list async
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("EquipmentProduct")]
        [HttpGet]
        public async Task<object> GetProductMiniInfoAsync([FromQuery] ProductQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.InvoiceMng, SCMRole.InvoiceObs };

            query.ProductType = ProductType.Equipment;

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
        [Route("supplier")]
        [HttpGet]
        public async Task<object> GetSupplierAsync([FromQuery] SupplierQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.InvoiceMng, SCMRole.InvoiceObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _supplierService.GetSuppliersAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Waiting For Add Invoice Badge Count
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetWaitingForAddInvoiceBadgeCount")]
        public async Task<object> GetWaitingForAddInvoiceBadgeCountAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.InvoiceMng, SCMRole.InvoiceObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _invoiceService.GetWaitingForAddInvoiceBadgeCountAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Waiting Receipt Or ReceiptReject For add Invoice
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("pendingForInvoice")]
        public async Task<object> GetWaitingReceiptOrReceiptRejectForInvoiceAsync([FromQuery] WaitingForInvoiceQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.InvoiceMng, SCMRole.InvoiceObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _invoiceService.GetWaitingReceiptOrReceiptRejectForInvoiceAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get all waiting item (receipt. receipt reject, popart) for add invoice 
        /// </summary>
        /// <param name="receiptId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("{receiptId:long}/pendingForAddInvoice")]
        public async Task<object> GetWaitingItemForAddNewInvoiceAsync(long receiptId, [FromQuery] GetWaitingInvoiceQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.InvoiceMng, SCMRole.InvoiceObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            switch (query.WaitingForInvoiceType)
            {
                case WaitingForInvoiceType.Receipt:
                    var serviceResult1 = await _invoiceService.GetReceiptByIdForAddNewInvoiceAsync(authenticate, receiptId);
                    return serviceResult1.ToWebApiResultVCore(authenticate.language);
                case WaitingForInvoiceType.ReceiptReject:
                    var serviceResult2 = await _invoiceService.GetReceiptRejectByIdForAddNewInvoiceAsync(authenticate, receiptId);
                    return serviceResult2.ToWebApiResultVCore(authenticate.language);

                default:
                    return new ServiceResult<bool>(false, false,
                                      new List<ServiceMessage>
                                          {new ServiceMessage(MessageType.Error, MessageId.InputDataValidationError)})
                              .ToWebApiResultVCore(authenticate.language);
            }
        }

        /// <summary>
        /// add invoice (source: receipt, receiptReject, poPart)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="receiptId"></param>
        /// <returns></returns>
        [HttpPost,Route("{receiptId:long}")]
        public async Task<object> AddInvoiceAsync(long receiptId,[FromBody] AddInvoiceDto model)
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


            authenticate.Roles = new List<string> { SCMRole.InvoiceMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            switch (model.WaitingForInvoiceType)
            {
                case Domain.Enum.WaitingForInvoiceType.Receipt:
                    var serviceResult = await _invoiceService.AddInvoiceByReceiptAsync(authenticate, receiptId, model);
                    return serviceResult.ToWebApiResultVCore(authenticate.language);

                case Domain.Enum.WaitingForInvoiceType.ReceiptReject:
                    var serviceResult1 = await _invoiceService.AddInvoiceByReceiptRejectAsync(authenticate,receiptId, model);
                    return serviceResult1.ToWebApiResultVCore(authenticate.language);

   

                default:
                    return
                                new ServiceResult<bool>(false, false,
                                        new List<ServiceMessage>
                                            {new ServiceMessage(MessageType.Error, MessageId.InputDataValidationError)})
                                .ToWebApiResultVCore(authenticate.language);
            }

        }

        /// <summary>
        /// get invoice list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetsInvoiceAsync([FromQuery] InvoiceQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.InvoiceMng, SCMRole.InvoiceObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _invoiceService.GetsInvoiceAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Get Invoice item details
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <returns></returns>
        [HttpGet, Route("{invoiceId:long}")]
        public async Task<object> GetInvoiceByIdAsync(long invoiceId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.InvoiceMng, SCMRole.InvoiceObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, invoiceId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _invoiceService.GetInvoiceByIdAsync(authenticate, invoiceId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download Invoice Attachment 
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet, Route("{invoiceId:long}/attachment/Download")]
        public async Task<object> DownloadInvoiceAttachmentAsync(long invoiceId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.InvoiceMng, SCMRole.InvoiceObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _invoiceService.DownloadInvoiceAttachmentAsync(authenticate, invoiceId, fileSrc);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, streamResult.ContentType,streamResult.FileName);

        }

    }
}