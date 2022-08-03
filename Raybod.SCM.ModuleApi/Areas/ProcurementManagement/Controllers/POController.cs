using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.PO;
using Raybod.SCM.DataTransferObject.Product;
using Raybod.SCM.DataTransferObject.ProductGroup;
using Raybod.SCM.DataTransferObject.Supplier;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.PurchaseManagement.Controllers
{
    [Route("api/procurementManagement/PO")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class POController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly IPOService _poService;
        private readonly IProductGroupService _productGroupService;
        private readonly IProductService _productService;
        private readonly ILogger<POController> _logger;

        public POController(ISupplierService supplierService,
            IPOService poService,
            IProductGroupService productGroupService,
            IProductService productService,
            ILogger<POController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _productGroupService = productGroupService;
            _productService = productService;
            _supplierService = supplierService;
            _poService = poService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// Get Product Group list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("ProductGroup")]
        [HttpGet]
        public async Task<object> GetProductGroupAsync([FromQuery] string query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

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
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            query.ProductType = Domain.Enum.ProductType.Equipment;

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productService.GetProductMiniInfoAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Supplier list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("supplier")]
        [HttpGet]
        public async Task<object> GetSuppliersAsync([FromQuery] SupplierQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _supplierService.GetSuppliersAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get pending po badge
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("pendingBadge")]
        public async Task<object> GetPOListBadgeAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poService.GetPOListBadgeAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get pending po list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("pending")]
        public async Task<object> GetPOPendingAsync([FromQuery] POQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poService.GetPOPendingAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get pending po item details
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpGet, Route("{poId:long}/pending")]
        public async Task<object> GetPOPendingByPOIdAsync(long poId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poService.GetPOPendingByPOIdAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get pending poSubject of prcontract
        /// </summary>
        /// <param name="prContractId"></param>
        /// <returns></returns>
        [HttpGet, Route("{prContractId:long}/pendingSubject")]
        public async Task<object> GetPOPendingSubjectByPRContractIdAsync(long prContractId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poService.GetPOPendingSubjectByPRContractIdAsync(authenticate, prContractId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add new po
        /// </summary>
        /// <param name="prContractId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{prContractId:long}/Register")]
        public async Task<object> AddPOAsync(long prContractId, [FromBody] AddPODto model)
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

            
            authenticate.Roles = new List<string> { SCMRole.POMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poService.AddPOAsync(authenticate, prContractId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Inprogress Pos list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("inProgress")]
        public async Task<object> GetInprogressPOAsync([FromQuery] POQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poService.GetInprogressPOAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Compeleted Po list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Delivered")]
        public async Task<object> GetDeliverdPOAsync([FromQuery] POQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poService.GetDeliverdPOAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Get all po list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("PoList")]
        public async Task<object> GetAllPOAsync([FromQuery] POQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poService.GetAllPOAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Get PO Status Logs 
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpGet, Route("{poId:long}/poStatusLog")]
        public async Task<object> GetPoStatusLogsAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poService.GetPoStatusLogsAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get po details
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpGet, Route("{poId:long}")]
        public async Task<object> GetPODetailsByPOIdAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poService.GetPODetailsByPOIdAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get po subjects
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpGet, Route("poSubjects")]
        public async Task<object> GetPOSubjectsByPOIdAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poService.GetPOSubjectsByPOIdAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

       

        /// <summary>
        /// get po attachment list
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpGet, Route("{poId:long}/attachment")]
        public async Task<object> GetPoAttachmentByPOIdAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.POMng,
                SCMRole.POObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poService.GetPoAttachmentByPOIdAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add po attachment
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpPost, Route("{poId:long}/attachment")]
        public async Task<object> AddPOAttachmentAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var files = HttpContext.Request.Form.Files;
            var serviceResult = await _poService.AddPOAttachmentAsync(authenticate, poId, files);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Remove PO Attachment
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpDelete, Route("{poId:long}/attachment")]
        public async Task<object> RemovePOAttachmentByPoIdAsync(long poId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poService.RemovePOAttachmentByPoIdAsync(authenticate, poId, fileSrc);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download PO Attachment
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet, Route("{poId:long}/poAttachment/download")]
        public async Task<object> DownloadPOAttachmentAsync(long poId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.POMng,
                SCMRole.POObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _poService.DownloadPOAttachmentAsync(authenticate, poId, fileSrc);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, "application/octet-stream");
        }


        /// <summary>
        /// cancel po 
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpPut, Route("{poId:long}/cancelPo")]
        public async Task<object> CancelPoAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poService.CancelPoAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}