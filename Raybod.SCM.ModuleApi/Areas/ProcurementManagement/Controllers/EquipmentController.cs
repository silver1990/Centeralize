using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Bom;
using Raybod.SCM.DataTransferObject.Mrp;
using Raybod.SCM.DataTransferObject.Product;
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
    [Route("api/procurementManagement/equipment")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class EquipmentController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<EquipmentController> _logger;
        private readonly IProductGroupService _productGroupService;
        private readonly IBomProductService _bomService;
        private readonly IMasterMrReportService _masterMrReportService;
        private readonly IDocumentRevisionService _revisionService;

        public EquipmentController(IProductService productService, ILogger<EquipmentController> logger,IHttpContextAccessor httpContextAccessor, IProductGroupService productGroupService, IBomProductService bomService, IMasterMrReportService masterMrReportService, IDocumentRevisionService revisionService)
        {
            _productService = productService;
            _logger = logger;
            _productGroupService = productGroupService;
            _bomService = bomService;
            _masterMrReportService = masterMrReportService;
            _revisionService = revisionService;
           
               
           
        }


        /// <summary>
        /// Get Master Mr By ContractCode 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("masterList")]
        public async Task<object> GetMasterMrByContractCodeAsync([FromQuery] MasterMRQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.BomMng, SCMRole.BomObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _masterMrReportService.GetMasterMrByContractCodeAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get bom list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("bomList")]
        public async Task<object> GetBomAsync([FromQuery] BomQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.BomMng, SCMRole.BomObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _bomService.GetBomAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Get ProductGroup item
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("productGroupList")]
        public async Task<object> GetProductGroupAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.BomMng, SCMRole.BomObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productGroupService.GetProductGroupMiniInfoAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpPost, Route("productGroup/{productGroupId:int}/addProduct")]
        public async Task<object> AddProductAsync(int productGroupId, [FromBody] List<AddProductWithBomDto> model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.BomMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productService.AddProductWithBomAsync(authenticate, productGroupId, model);

            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Product Unit 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("productUnitList")]
        public async Task<object> GetProductUnitAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.BomMng, SCMRole.BomObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productService.GetProductUnitAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add new Product Unit 
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("addProductUnit")]
        public async Task<object> AddProductUnitAsync(string unit)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.BomMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, unit);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productService.AddProductUnitAsync(authenticate, unit);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        [HttpPost, Route("product/{productId:int}/addSubset")]
        public async Task<object> AddSubsetAsync(int productId, [FromBody] List<AddProductSubsetDto> model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.BomMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productService.AddSubsetProductAsync(authenticate, productId, model);

            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        [HttpGet, Route("product/{productId:int}/getProductInfo")]
        public async Task<object> GetProductInfoAsync(int productId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.BomMng, SCMRole.BomObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productService.GetProductInfoAsync(authenticate, productId);

            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        //[HttpGet, Route("product/{productId:int}/getDuplicateInfo")]
        //public async Task<object> GetDuplicateInfoAsync(int productId)
        //{

        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { SCMRole.ProductMng };

        //    var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
        //    _logger.LogWarning(logInformation.InformationText, logInformation.Args);

        //    var serviceResult = await _productService.GetDuplicateInfoAsync(authenticate, productId);

        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}

        //[HttpPost, Route("product/{productId:int}/createDuplicateInfo")]
        //public async Task<object> AddDuplicateAsync(int productId, [FromBody] CreateDuplicateDto model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return
        //            new ServiceResult<bool>(false, false,
        //                    new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
        //                .ToWebApiResultVCore(authenticate.language,ModelState);
        //    }
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { SCMRole.ProductMng };

        //    var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
        //    _logger.LogWarning(logInformation.InformationText, logInformation.Args);

        //    var serviceResult = await _productService.AddDuplicateAsync(authenticate, productId, model);

        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}

        [HttpGet, Route("productGroup/{productGroupId:int}/getProductListInfo")]
        public async Task<object> GetProductListInfoAsync(int productGroupId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.BomMng, SCMRole.BomObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productService.GetProductListInfoAsync(authenticate, productGroupId);

            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        

        [HttpGet, Route("productDocuments/{productId:int}")]
        public async Task<object> GetDocumentByProductIdBaseOnContractAsync(int productId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>
            {   SCMRole.BomMng,
                SCMRole.BomObs,
                SCMRole.MrpMng,
                SCMRole.MrpObs,
                SCMRole.RFPMng,
                SCMRole.RFPObs,
                SCMRole.RFPTechMng,
                SCMRole.RFPTechObs,
                SCMRole.RFPTechEvaluationMng,
                SCMRole.RFPCommercialMng,
                SCMRole.RFPCommercialEvaluationMng,
                SCMRole.RFPCommercialObs,
                SCMRole.RFPProFormaMng,
                SCMRole.RFPProFromaObs,
                SCMRole.RFPWinnerMng,
                SCMRole.RFPWinnerObs,
                SCMRole.PurchaseRequestReg,
                SCMRole.PurchaseRequestConfirm,
                SCMRole.PurchaseRequestObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, productId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _revisionService.GetDocumentByProductIdBaseOnContractAsync(authenticate, productId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet, Route("{documentId:long}/ProductDocuments/{productId:int}/downloadRevisionAttachForProduct")]
        public async Task<object> GetLastDocumentRevisionAttachAzZipFileByProductIdAsync(long documentId, int productId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>
            {   SCMRole.BomMng,
                SCMRole.BomObs,
                SCMRole.MrpMng,
                SCMRole.MrpObs,
                SCMRole.RFPMng,
                SCMRole.RFPObs,
                SCMRole.RFPTechMng,
                SCMRole.RFPTechObs,
                SCMRole.RFPTechEvaluationMng,
                SCMRole.RFPCommercialMng,
                SCMRole.RFPCommercialEvaluationMng,
                SCMRole.RFPCommercialObs,
                SCMRole.RFPProFormaMng,
                SCMRole.RFPProFromaObs,
                SCMRole.RFPWinnerMng,
                SCMRole.RFPWinnerObs,
                SCMRole.PurchaseRequestReg,
                SCMRole.PurchaseRequestConfirm,
                SCMRole.PurchaseRequestObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, documentId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _revisionService.GetLastDocumentRevisionAttachAsZipFileByProductIdAsync(authenticate, documentId, productId);
            if (streamResult == null)
                return NotFound();



            return File(streamResult.ArchiveFile, "application/zip", streamResult.FileName);
        }


        [HttpGet, Route("{productId:int}/ProductDocuments/downloadRevisionsAttachForProduct")]
        public async Task<object> GetLastDocumentRevisionsAttachAzZipFileByProductIdAsync(int productId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>
            {   SCMRole.BomMng,
                SCMRole.BomObs,
                SCMRole.MrpMng,
                SCMRole.MrpObs,
                SCMRole.RFPMng,
                SCMRole.RFPObs,
                SCMRole.RFPTechMng,
                SCMRole.RFPTechObs,
                SCMRole.RFPTechEvaluationMng,
                SCMRole.RFPCommercialMng,
                SCMRole.RFPCommercialEvaluationMng,
                SCMRole.RFPCommercialObs,
                SCMRole.RFPProFormaMng,
                SCMRole.RFPProFromaObs,
                SCMRole.RFPWinnerMng,
                SCMRole.RFPWinnerObs,
                SCMRole.PurchaseRequestReg,
                SCMRole.PurchaseRequestConfirm,
                SCMRole.PurchaseRequestObs
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, productId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _revisionService.GetLastDocumentRevisionsAttachAsZipFileByProductIdAsync(authenticate, productId);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, "application/zip", streamResult.FileName);
        }

        [HttpPut, Route("Product/{productId:int}/EditProduct")]
        public async Task<object> EditProductAsync(int productId, [FromBody] EditProductDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }
            
            authenticate.Roles = new List<string> { SCMRole.BomMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productService.EditProductAsync(authenticate, productId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet, Route("{productId:int}/getBomProductByProductId")]
        public async Task<object> GetBomProductByProductIdAsync(int productId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.BomMng, SCMRole.BomObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productService.GetBomProductByProductIdAsync(authenticate, productId);

            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        [HttpPut, Route("bomProducts/{bomId:long}/editBomProducts")]
        public async Task<object> GetBomProductByProductIdAsync(long bomId, [FromBody] CreateBomFormAnotherBomDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.BomMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productService.EditBomProductAsync(authenticate, bomId, model);

            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet, Route("bomProducts/{bomId:long}/archive")]
        public async Task<object> GetBomProductArchiveByBomIdAsync(long bomId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.BomMng, SCMRole.BomObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productService.GetBomProductArchiveByBomIdAsync(authenticate, bomId);

            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        //[HttpPost, Route("bomProducts/{bomId:long}/addBomProducts")]
        //public async Task<object> AddBomProductsAsync(long bomId,[FromBody] CreateBomFormAnotherBomDto model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return
        //            new ServiceResult<bool>(false, false,
        //                    new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
        //                .ToWebApiResultVCore(authenticate.language,ModelState);
        //    }
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { SCMRole.ProductMng };

        //    var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
        //    _logger.LogWarning(logInformation.InformationText, logInformation.Args);

        //    var serviceResult = await _productService.AddBomProductsAsync(authenticate, bomId,model);

        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}
    }
}
