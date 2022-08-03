using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Receipt;
using Raybod.SCM.DataTransferObject.Warehouse;
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
    [Route("api/procurementManagement/warehouseDespatch")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class WarehouseOutputController : ControllerBase
    {
        private readonly IReceiptService _receiptService;
        private readonly IWarehouseService _warehouseService;
        private readonly ILogger<WarehouseOutputController> _logger;

        public WarehouseOutputController(
            IReceiptService receiptService,
            ILogger<WarehouseOutputController> logger,IHttpContextAccessor httpContextAccessor, IWarehouseService warehouseService)
        {
            _receiptService = receiptService;
            _logger = logger;
            _warehouseService = warehouseService;
           
               
           
        }

        /// <summary>
        /// Get Waiting Receipt For Despatch List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("WaitnigwarehouseDespatch")]
        public async Task<object> GetWaitingReceiptForRejectListAsync([FromQuery] ReceiptQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _receiptService.GetWaitingReceiptForRejectListAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        ///// <summary>
        ///// Get Waiting WarehouseDespatch Info By receiptId For reject
        ///// </summary>
        ///// <param name="receiptId"></param>
        ///// <returns></returns>
        //[HttpGet, Route("{receiptId:long}/ForDespatch")]
        //public async Task<object> GetWaitingReceiptForRejectInfoByReceiptIdAsync(long receiptId)
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseObs };

        //    var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
        //    _logger.LogWarning(logInformation.InformationText, logInformation.Args);

        //    var serviceResult = await _receiptService.GetWaitingReceiptForRejectInfoByReceiptIdAsync(authenticate, receiptId);
        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}

        ///// <summary>
        ///// Add WarehouseDespatch 
        ///// </summary>
        ///// <param name="receiptId"></param>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[HttpPost, Route("{receiptId:long}/Despatch")]
        //public async Task<object> AddReceiptRejectAsync(long receiptId, [FromBody] AddReceiptRejectDto model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return
        //          new ServiceResult<bool>(false, false,
        //                  new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
        //              .ToWebApiResultVCore(authenticate.language,ModelState);
        //    }

        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { SCMRole.WarehouseMng };

        //    var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
        //    _logger.LogWarning(logInformation.InformationText, logInformation.Args);

        //    var serviceResult = await _receiptService.AddReceiptRejectAsync(authenticate, receiptId, model);
        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}
      
        /// <summary>
        /// Get WarehouseDespatch List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("Despatch")]
        public async Task<object> GetReceiptRejectListAsync([FromQuery] ReceiptQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _receiptService.GetReceiptRejectListAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get WarehouseDespatch Info By receiptRejeceId 
        /// </summary>
        /// <param name="receiptRejeceId"></param>
        /// <returns></returns>
        [HttpGet, Route("Despatch/{receiptRejeceId:long}")]
        public async Task<object> GetReceiptRejectInfoByIdAsync(long receiptRejeceId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _receiptService.GetReceiptRejectInfoByIdAsync(authenticate, receiptRejeceId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download WarehouseDespatch Attachment
        /// </summary>
        /// <param name="receiptRejeceId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet, Route("Despatch/{receiptRejeceId:long}/Download")]
        public async Task<object> DownloadReceiptRejectAttachmentAsync(long receiptRejeceId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseQCMng, SCMRole.WarehouseObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _receiptService.DownloadReceiptRejectAttachmentAsync(authenticate, receiptRejeceId, fileSrc);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, "application/octet-stream");

        }
        [HttpGet, Route("productGroup/{productGroupId:int}/getWarehouseProductListInfo")]
        public async Task<object> GetWarehouseProductListInfoAsync(int productGroupId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.BomMng, SCMRole.BomObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.GetWarehouseProductListInfoAsync(authenticate, productGroupId);

            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get Warehouse Requisition confirm user  list 
        /// </summary>
        /// <param name="productGroupId"></param>
        /// <returns></returns>
        [HttpGet, Route("{productGroupId:int}/workFlowRegisterUsers")]
        public async Task<object> GetConfirmUserAsync(int productGroupId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WareHouseOutputRequestReg };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, productGroupId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.GetConfirmationUserListAsync(authenticate, productGroupId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add Warehouse Requisition
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("warehouseRequisition")]
        public async Task<object> AddWarehouseOutputRequestAsync([FromBody] AddWarehouseOutputRequestDto model)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                  new ServiceResult<bool>(false, false,
                          new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                      .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.WareHouseOutputRequestReg };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.AddWarehouseOutputRequest(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Warehouse Requisition pending confiem  item by requestId
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [HttpGet, Route("workflowConfirm/{requestId:long}/pendingConfirm")]
        public async Task<object> GetPendingWarehouseOutputRequestConfiemByPurchaseRequestIdAsync(long requestId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.WareHouseOutputRequestConfirm,SCMRole.WareHouseOutputRequestReg,SCMRole.WareHouseOutputRequestObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _warehouseService.GetPendingConfirmWarehouseOutputRequestByPurchaseRequestIdAsync(authenticate, requestId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        [HttpPost, Route("workflowConfirm/{requestId:long}/ConfirmationTask")]
        public async Task<object> SetUserConfirmOwnWarehouseOutputRequestTaskAsync(long requestId, [FromBody] AddWarehouseRequestConfirmationAnswerDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.WareHouseOutputRequestConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _warehouseService.SetUserConfirmOwnWarehouseOutputRequestTaskAsync(authenticate, requestId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Get Pending Warehouse Requisition List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("pendingwarehouseRequisition")]
        public async Task<object> GetPendingwarehouseRequisitionListAsync([FromQuery] WarehouseOutputQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WareHouseOutputRequestObs, SCMRole.WareHouseOutputRequestReg, SCMRole.WareHouseOutputRequestConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.GetPendingwarehouseRequisitionListAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Get Warehouse Requisition List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("warehouseRequisitionList")]
        public async Task<object> GetWarehouseRequisitionListAsync([FromQuery] WarehouseOutputQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WareHouseOutputRequestObs, SCMRole.WareHouseOutputRequestReg, SCMRole.WareHouseOutputRequestConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.GetWarehouseRequisitionListAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Get Warehouse Requisition List
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [HttpGet, Route("warehouseRequisition/{requestId:long}")]
        public async Task<object> GetWarehouseRequisitionByRequestIdAsync(long requestId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WareHouseOutputRequestObs, SCMRole.WareHouseOutputRequestReg, SCMRole.WareHouseOutputRequestConfirm };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, requestId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.GetWarehouseRequisitionByRequestIdAsync(authenticate, requestId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Pending Warehouse Despatch List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("pendingForDespatch")]
        public async Task<object> GetConfirmWarehouseRequisitionListAsync([FromQuery] WarehouseOutputQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseDispatchMng, SCMRole.WarehouseDispatchObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.GetConfrimWarehouseRequisitionListAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Pending WarehouseDespatch Info By Request Id For Despatch
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [HttpGet, Route("{requestId:long}/pendingForDespatch")]
        public async Task<object> GetWaitingRequestForDespatchInfoByRequestIdAsync(long requestId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseDispatchMng, SCMRole.WarehouseDispatchObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.GetWaitingRequestForDespatchInfoByRequestIdAsync(authenticate, requestId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add WarehouseDespatch 
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{requestId:long}/Despatch")]
        public async Task<object> AddWarehouseDespatchAsync(long requestId, [FromBody] AddWarehouseDespatchDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                  new ServiceResult<bool>(false, false,
                          new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                      .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.WarehouseDispatchMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.AddWarehouseDespatchAsync(authenticate, requestId, model);

            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// cancel WarehouseDespatch 
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{requestId:long}/cancelDespatch")]
        public async Task<object> CancelWarehouseDespatchAsync(long requestId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                  new ServiceResult<bool>(false, false,
                          new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                      .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.WarehouseDispatchMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.CancelWarehouseDespatchAsync(authenticate, requestId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Get Despatch list 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("warehouseDespatchList")]
        public async Task<object> GetWarehouseDespatchListAsync([FromQuery]WarehouseDespatchQueryDto query)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseDispatchMng, SCMRole.WarehouseDispatchObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.GetWarehouseDespatchListAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Warehouse Despatch by Despatch Id List
        /// </summary>
        /// <param name="despatchId"></param>
        /// <returns></returns>
        [HttpGet, Route("warehouseDespatch/{despatchId:long}")]
        public async Task<object> GetWarehouseDespatchByRequestIdAsync(long despatchId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseDispatchMng, SCMRole.WarehouseDispatchObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, despatchId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.GetWarehouseDespatchByDespatchIdAsync(authenticate, despatchId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}