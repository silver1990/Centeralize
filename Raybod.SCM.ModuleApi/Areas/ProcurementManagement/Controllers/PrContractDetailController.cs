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
    public class PrContractDetailController : ControllerBase
    {
        private readonly IPRContractService _prContractService;
        private readonly IRFPService _rfpService;
        private readonly IProductService _productService;
        private readonly IProductGroupService _productGroupService;
        private readonly ISupplierService _supplierService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PrContractDetailController> _logger;

        public PrContractDetailController(
           IPRContractService prContractService,
           IRFPService rfpService,

           IProductService productService,
           IProductGroupService productGroupService,
           ISupplierService supplierService,
           IPaymentService paymentService,
           ILogger<PrContractDetailController> logger,IHttpContextAccessor httpContextAccessor)
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
        /// get prContract details
        /// </summary>
        /// <param name="prContractId"></param>
        /// <returns></returns>
        [HttpGet, Route("{prContractId:long}")]
        public async Task<object> GetPRContractByPRContractIdAsync(long prContractId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _prContractService.GetPRContractByPRContractIdAsync(authenticate, prContractId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// get prContract details
        /// </summary>
        /// <param name="prContractId"></param>
        /// <returns></returns>
        [HttpGet, Route("{prContractId:long}/editInfo")]
        public async Task<object> GetPRContractDetailsByPRContractIdAsync(long prContractId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _prContractService.GetPRContractDetailsByPRContractIdAsync(authenticate, prContractId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Get Pr Contract Pending Confiem  Item by PrContractId
        /// </summary>
        /// <param name="prContractId"></param>
        /// <returns></returns>
        [HttpGet, Route("workflowConfirm/{prContractId:long}/pendingConfirm")]
        public async Task<object> GetPendingPrContractConfiemByPrContractIdAsync(long prContractId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _prContractService.GetPendingConfirmPrContractByPrContractIdAsync(authenticate, prContractId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Get Pr Contract Pending Confiem  Item by PrContractId
        /// </summary>
        /// <param name="prContractId"></param>
        /// <returns></returns>
        [HttpGet, Route("workflowConfirm/{prContractId:long}/lastWorkFlow")]
        public async Task<object> GetLastPrContractWorkFlowByPrContractIdAsync(long prContractId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _prContractService.GetLastPrContractWorkFlowByPrContractIdAsync(authenticate, prContractId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// download prContract attachment
        /// </summary>
        /// <param name="prContractId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet, Route("{prContractId:long}/downloadAttachments")]
        public async Task<object> DownloadAttachmentAsync(long prContractId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _prContractService.DownloadAttachmentAsync(authenticate, prContractId, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }
        /// <summary>
        /// download prContract workflow attachment
        /// </summary>
        /// <param name="prContractConfirmWorkFlow"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet, Route("prConfirmWorkflow/{prContractConfirmWorkFlowId:long}/downloadAttachments")]
        public async Task<object> DownloadConfirmWorkFlowAttachmentAsync(long prContractConfirmWorkFlowId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _prContractService.DownloadConfirmWorkFlowAttachmentAsync(authenticate, prContractConfirmWorkFlowId, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }
        /// <summary>
        /// get prcontract attachment list
        /// </summary>
        /// <param name="prContractId"></param>
        /// <returns></returns>
        //[HttpGet, Route("{prContractId:long}/attachments")]
        //public async Task<object> GetPRContractAttachmentByPRContractIdAsync(long prContractId)
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

        //    var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
        //    _logger.LogWarning(logInformation.InformationText, logInformation.Args);

        //    var serviceResult = await _prContractService.GetPRContractAttachmentByPRContractIdAsync(authenticate, prContractId);
        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}
        [HttpGet, Route("{prContractId:long}/attachments")]
        public async Task<object> GetPRContractAttachmentByPRContractIdAsync(long prContractId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _prContractService.DownloadAttachmentAsync(authenticate, prContractId, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }
        /// <summary>
        /// add prcontract new attachment
        /// </summary>
        /// <param name="prContractId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("{prContractId:long}/attachment")]
        [HttpPost]
        public async Task<object> AddAttachment(long prContractId, [FromBody] List<AddAttachmentDto> model)
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

            var serviceResult = await _prContractService.AddAttachmentAsync(authenticate, prContractId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// remove prcontract attachment
        /// </summary>
        /// <param name="prContractId"></param>
        /// <param name="attachmentId"></param>
        /// <returns></returns>
        [Route("{prContractId:long}/attachment/{attachmentId:long}")]
        [HttpDelete]
        public async Task<object> RemoveAttachmentAsync(long prContractId, long attachmentId)
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

           
            authenticate.Roles = new List<string> { SCMRole.PrContractMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _prContractService.RemoveAttachmentAsync(authenticate, prContractId, attachmentId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// get prContract subjetcs and services
        /// </summary>
        /// <param name="prContractId"></param>
        /// <returns></returns>
        [HttpGet, Route("{prContractId:long}/prContractSubjects")]
        public async Task<object> GetPRContractSubjectsAndServiceByContractIdAsync(long prContractId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _prContractService.GetPRContractSubjectsAndServiceByContractIdAsync(authenticate, prContractId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get PRContract Term OF Payment 
        /// </summary>
        /// <param name="prContractId"></param>
        /// <returns></returns>
        [HttpGet, Route("{prContractId:long}/termOfPayment")]
        public async Task<object> GetPRContractTermOFPaymentByContractIdAsync(long prContractId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _prContractService.GetPRContractTermOFPaymentByContractIdAsync(authenticate, prContractId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }





        /// <summary>
        /// Get Report POSubject of prContract
        /// </summary>
        /// <param name="prContractId"></param>
        /// <returns></returns>
        [HttpGet, Route("{prContractId:long}/reportPOSubject")]
        public async Task<object> GetReportPOSubjectbyprContractIdAsync(long prContractId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractReg, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _prContractService.GetReportPOSubjectbyprContractIdAsync(authenticate, prContractId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Report PendingForPayment of PRContract
        /// </summary>
        /// <param name="prContractId"></param>
        /// <returns></returns>
        [HttpGet, Route("{prContractId:long}/reportPendigForPayment")]
        public async Task<object> GetReportPendingForPaymentByPRContractIdAsync(long prContractId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrContractMng, SCMRole.PrContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _paymentService.GetReportPendingForPaymentByPRContractIdAsync(authenticate, prContractId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

    }
}
