using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Product;
using Raybod.SCM.DataTransferObject.Receipt;
using Raybod.SCM.DataTransferObject.Supplier;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.Warehouse.Controllers
{
    [Route("api/procurementManagement/receipt")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class WarehouseInputController : ControllerBase
    {
        private readonly IReceiptService _receiptService;
        private readonly ISupplierService _supplierService;
        private readonly IProductGroupService _productGroupService;
        private readonly IProductService _productService;
        private readonly ILogger<WarehouseInputController> _logger;

        public WarehouseInputController(IReceiptService receiptService,
              ISupplierService supplierService,
            IProductGroupService productGroupService,
            IProductService productService,
            ILogger<WarehouseInputController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _receiptService = receiptService;
            _productGroupService = productGroupService;
            _productService = productService;
            _supplierService = supplierService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// get productgroup list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("ProductGroup")]
        [HttpGet]
        public async Task<object> GetProductGroupAsync([FromQuery] string query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseQCMng, SCMRole.WarehouseObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productGroupService.GetProductGroupAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get EquipmentProduct list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("EquipmentProduct")]
        [HttpGet]
        public async Task<object> GetProductMiniInfoAsync([FromQuery] ProductQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseQCMng, SCMRole.WarehouseObs };

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
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseQCMng, SCMRole.WarehouseObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _supplierService.GetSuppliersAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Waiting Receip Badge Count
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("waitingBadgeCount")]
        public async Task<object> GetReceiptWaitingBadgeCountAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseObs, SCMRole.WarehouseQCMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _receiptService.GetReceiptWaitingBadgeCountAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get waiting pack for receipt list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("waitingPack")]
        public async Task<object> GetWaitingPackForReceiptAsync([FromQuery] WaitingPackQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseObs, SCMRole.WarehouseQCMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _receiptService.GetWaitingPackForReceiptAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get waiting pack item for receipt
        /// </summary>
        /// <param name="packId"></param>
        /// <param name="isPart"></param>
        /// <param name="subjectProductId"></param>
        /// <returns></returns>
        [HttpGet, Route("waitingPack/{packId:long}")]
        public async Task<object> GetWaitingPackInfoByIdAsync(long packId, bool isPart, long? subjectProductId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseObs, SCMRole.WarehouseQCMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _receiptService.GetWaitingPackInfoByIdAsync(authenticate, packId, isPart, subjectProductId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add receipt 
        /// </summary>
        /// <param name="packId"></param>
        /// <param name="isPart"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("pack/{packId:long}")]
        public async Task<object> AddReceiptForPackAsync(long packId, [FromBody]List<AddReceiptProductDto> model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                  new ServiceResult<bool>(false, false,
                          new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                      .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _receiptService.AddReceiptForPackAsync(authenticate, packId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get waiting receipt list for QC
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("temporaryReceipt")]
        public async Task<object> GetWaitingReceiptForAddQCListAsync([FromQuery] ReceiptQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseObs, SCMRole.WarehouseQCMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _receiptService.GetWaitingReceiptForAddQCListAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get receipt list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetReceiptListAsync([FromQuery] ReceiptQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseObs, SCMRole.WarehouseQCMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _receiptService.GetReceiptListAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get receipt item for add Qc
        /// </summary>
        /// <param name="receiptId"></param>
        /// <returns></returns>
        [HttpGet, Route("{receiptId:long}/ForSetFinal")]
        public async Task<object> GetReceiptInfoByIdForAddQCAsync(long receiptId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseObs, SCMRole.WarehouseQCMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _receiptService.GetReceiptInfoByIdForAddQCAsync(authenticate, receiptId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get receipt item details
        /// </summary>
        /// <param name="receiptId"></param>
        /// <returns></returns>
        [HttpGet, Route("{receiptId:long}")]
        public async Task<object> GetReceiptInfoByIdAsync(long receiptId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseObs, SCMRole.WarehouseQCMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _receiptService.GetReceiptInfoByIdAsync(authenticate, receiptId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add receipt qc
        /// </summary>
        /// <param name="receiptId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{receiptId:long}/addQualityControl")]
        public async Task<object> AddReceiptQualityControlAsync(long receiptId, [FromBody]AddQualityControlReceiptDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                  new ServiceResult<bool>(false, false,
                          new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                      .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            
            authenticate.Roles = new List<string> { SCMRole.WarehouseQCMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _receiptService.AddReceiptQualityControlAsync(authenticate, receiptId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// download receipt quality control attachment
        /// </summary>
        /// <param name="receiptId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet, Route("{receiptId:long}/qualityControl/Download")]
        public async Task<object> DownloadReceiptQualityControleAttachmentAsync(long receiptId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseQCMng, SCMRole.WarehouseObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _receiptService.DownloadReceiptQualityControleAttachmentAsync(authenticate, receiptId, fileSrc);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream,streamResult.ContentType,streamResult.FileName);

        }

    }
}