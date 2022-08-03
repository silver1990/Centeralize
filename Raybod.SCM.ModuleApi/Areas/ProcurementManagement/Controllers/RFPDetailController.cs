using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.PurchaseRequest;
using Raybod.SCM.DataTransferObject.RFP;
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
    public class RFPDetailController : ControllerBase
    {
        private readonly IPurchaseRequestService _purchaseRequestService;
        private readonly IRFPService _rfpService;
        private readonly ISupplierService _supplierService;
        private readonly ILogger<RFPDetailController> _logger;

        public RFPDetailController(
           IPurchaseRequestService purchaseRequestService,
           IRFPService rfpService,
           ISupplierService supplierService,
           ILogger<RFPDetailController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _purchaseRequestService = purchaseRequestService;
            _rfpService = rfpService;
            _supplierService = supplierService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// get rfp inquery list
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="inqueryType"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/RFPInquery")]
        [HttpGet]
        public async Task<object> GetRFPInquery(long rfpId, RFPInqueryType inqueryType)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng, SCMRole.RFPObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetRFPInqueryByRFPIdAsync(authenticate, rfpId, inqueryType);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get RFP Evaluation list
        /// </summary>
        /// <param name="rfpId"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/RFPEvaluation")]
        [HttpGet]
        public async Task<object> GetRFPEvaluation(long rfpId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPWinnerMng, SCMRole.RFPWinnerObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetRFPEvaluationAsync(authenticate, rfpId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Get pro forma list for select winner
        /// </summary>
        /// <param name="rfpId"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/proformaList")]
        [HttpGet]
        public async Task<object> GetProFormaList(long rfpId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPWinnerMng, SCMRole.ProductMng,SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetRFPProFormaListAsync(authenticate, rfpId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Set RFP Supplier Winner
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="supplierIds"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/setWinner")]
        [HttpPost]
        public async Task<object> SetRFPSupplierWinner(long rfpId, [FromBody] List<int> supplierIds)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPWinnerMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, supplierIds);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.SetRFPSupplierWinner(authenticate, rfpId, supplierIds);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// DeActive RFPItem  
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="rfpItemId"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/RFPItem/{rfpItemId:long}/DeActive")]
        [HttpPost]
        public async Task<object> DeActiveRFPItem(long rfpId, long rfpItemId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, rfpItemId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.DeActiveRFPItemByIdAsync(authenticate, rfpId, rfpItemId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add new rfpItem
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/RFPItem")]
        [HttpPost]
        public async Task<object> AddRFPItem(long rfpId, [FromBody] List<AddRFPItemDto> model)
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

            var serviceResult = await _rfpService.AddRFPItemByRFPIdAsync(authenticate, rfpId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// edit rfp inquery
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="inqueryType"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/RFPInquery")]
        [HttpPut]
        public async Task<object> EditRFPInquery(long rfpId, RFPInqueryType inqueryType, [FromBody] List<RFPInqueryInfoDto> model)
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

            var serviceResult = await _rfpService.EditRFPInqueryByRFPIdAsync(authenticate, rfpId, inqueryType, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// DeActive RFP Supplier
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/supplier/{supplierId:int}/DeActive")]
        [HttpPost]
        public async Task<object> DeActiveRFPSupplier(long rfpId, int supplierId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.DeActiveRFPSupplierBySupplierIdAsync(authenticate, rfpId, supplierId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add or remove rfp supplier
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="supplierIds"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/supplier")]
        [HttpPut]
        public async Task<object> EditRFPSupplier(long rfpId, [FromBody] List<AddRFPSupplierDto> supplierIds)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, supplierIds);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.EditRFPSupplierAsync(authenticate, rfpId, supplierIds);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

       
        
        //[HttpGet, Route("{rfpId:long}/techInquery/{inqueryId:long}/downloadAttachment")]
        //public async Task<object> DownloadTechInqueryAttachmentAsync(long rfpId, long inqueryId, string fileSrc)
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { SCMRole.RFPTechObs, SCMRole.RFPTechMng, SCMRole.RFPTechEvaluationMng };

        //    var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
        //    _logger.LogWarning(logInformation.InformationText, logInformation.Args);

        //    var streamResult = await _rfpService.DownloadRFPInqueryAttachmentAsync(authenticate, rfpId, inqueryId, RFPInqueryType.TechnicalInquery, fileSrc);
        //    if (streamResult == null)
        //        return NotFound();
        //    return File(streamResult.Stream, "application/octet-stream");

        //}

        //[HttpGet, Route("{rfpId:long}/commercialInquery/{inqueryId:long}/downloadAttachment")]
        //public async Task<object> DownloadCommercialInqueryAttachmentAsync(long rfpId, long inqueryId, string fileSrc)
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { SCMRole.RFPCommercialEvaluationMng, SCMRole.RFPCommercialMng, SCMRole.RFPCommercialObs };

        //    var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
        //    _logger.LogWarning(logInformation.InformationText, logInformation.Args);

        //    var streamResult = await _rfpService.DownloadRFPInqueryAttachmentAsync(authenticate, rfpId, inqueryId, RFPInqueryType.CommercialInquery, fileSrc);
        //    if (streamResult == null)
        //        return NotFound();
        //    return File(streamResult.Stream, "application/octet-stream");

        //}



        [HttpGet, Route("{rfpId:long}/rfpAttachment/download")]
        public async Task<object> DownloadRFPAttachmentAsync(long rfpId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng, SCMRole.RFPObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _rfpService.DownloadRFPAttachmentAsync(authenticate, rfpId, fileSrc);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);

        }

        /// <summary>
        /// Get Waiting PRItem list For Add new RFP 
        /// </summary>
        /// <param name="productGroupId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("PR/WaitingPRItems/{productGroupId:int}")]
        [HttpGet]
        public async Task<object> GetWaitingPRItemForAddRFPByProductGroupIdAsync(int productGroupId, [FromQuery] PurchaseRequestQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng, SCMRole.RFPObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _purchaseRequestService.GetWaitingPRItemForAddRFPByProductGroupIdAsync(authenticate, productGroupId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Cancel rfp
        /// </summary>
        /// <param name="rfpId"></param>
        /// <returns></returns>
        [HttpPut, Route("{rfpId:long}/cancelRfp")]
        public async Task<object> CancelRFPAsync(long rfpId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.CancelRFPAsync(authenticate, rfpId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
