using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    public class RFPProposalController : ControllerBase
    {

        private readonly IPurchaseRequestService _purchaseRequestService;
        private readonly IRFPService _rfpService;
        private readonly ISupplierService _supplierService;
        private readonly ILogger<RFPProposalController> _logger;

        public RFPProposalController(
           IPurchaseRequestService purchaseRequestService,
           IRFPService rfpService,
           ISupplierService supplierService,
           ILogger<RFPProposalController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _purchaseRequestService = purchaseRequestService;
            _rfpService = rfpService;
            _supplierService = supplierService;
            _logger = logger;
           
               
           
        }
        
        /// <summary>
        /// Get RFP Status log
        /// </summary>
        /// <param name="rfpId"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/getStatus")]
        [HttpGet]
        public async Task<object> GetRFPStatus(long rfpId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPMng, SCMRole.RFPObs, SCMRole.RFPWinnerMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetRFPStatusByRFPIdAsync(authenticate, rfpId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Supplier Tech Proposal
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/supplier/{supplierId:int}/TechProposal")]
        [HttpGet]
        public async Task<object> GetSupplierTechProposal(long rfpId, int supplierId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPTechObs, SCMRole.RFPTechMng, SCMRole.RFPTechEvaluationMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, supplierId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetRFPSupplierInqueryAsync(authenticate, rfpId, supplierId, RFPInqueryType.TechnicalInquery);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Supplier Commercial Proposal
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/supplier/{supplierId:int}/CommercialProposal")]
        [HttpGet]
        public async Task<object> GetSupplierCommercialProposal(long rfpId, int supplierId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPCommercialEvaluationMng, SCMRole.RFPCommercialMng, SCMRole.RFPCommercialObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, supplierId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetRFPSupplierInqueryAsync(authenticate, rfpId, supplierId, RFPInqueryType.CommercialInquery);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add Supplier Tech Proposal
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="supplierId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/supplier/{supplierId:int}/AddProposal")]
        [HttpPost]
        public async Task<object> AddSupplierProposal(long rfpId, int supplierId, [FromBody] List<AddSupplierProposalDto> model, RFPInqueryType inqueryType)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (inqueryType == RFPInqueryType.TechnicalInquery)
                authenticate.Roles = new List<string> { SCMRole.RFPTechMng };
            else if (inqueryType == RFPInqueryType.CommercialInquery)
                authenticate.Roles = new List<string> { SCMRole.RFPCommercialMng };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.AddRFPSupplierProposalAsync(authenticate, rfpId,
                supplierId, inqueryType, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Add Supplier Evaluation Tech Proposal
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="supplierId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/supplier/{supplierId:int}/EvaluationProposal")]
        [HttpPost]
        public async Task<object> AddSupplierEvaluationProposalAsync(long rfpId, int supplierId, [FromBody] AddRFPSupplierEvaluationDto model, RFPInqueryType inqueryType)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (inqueryType == RFPInqueryType.TechnicalInquery)
                authenticate.Roles = new List<string> { SCMRole.RFPTechEvaluationMng };
            else if (inqueryType == RFPInqueryType.CommercialInquery)
                authenticate.Roles = new List<string> { SCMRole.RFPCommercialEvaluationMng };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.AddSupplierEvaluationProposalAsync(authenticate, rfpId,
                supplierId, inqueryType, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

       

        [HttpGet, Route("{rfpSupplier:long}/rfpProforma/{proFormaId:long}/downloadAttachment")]
        public async Task<object> DownloadProFormaAttachmentAsync(long rfpSupplier, long proFormaId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPProFormaMng, SCMRole.RFPProFromaObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _rfpService.DownloadRFPProFormaAttachmentAsync(authenticate, rfpSupplier, proFormaId, fileSrc);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);

        }


        [HttpGet, Route("proposalAttachment/{proposalId:long}/downloadAttachment")]
        public async Task<object> DownloadProposalAttachmentAsync(long proposalId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPTechObs, SCMRole.RFPTechMng, SCMRole.RFPTechEvaluationMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _rfpService.DownloadRFPSupplierProposalAttachmentAsync(authenticate, proposalId, fileSrc);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);

        }


        /// <summary>
        /// Get rfp proforma detail
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="rfpSupplierId"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/RFPSupplier/{rfpSupplierId:long}/proFormaDetail")]
        [HttpGet]
        public async Task<object> GetRFPProFromaDetailAsync(long rfpId, long rfpSupplierId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPProFormaMng, SCMRole.RFPProFromaObs };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.GetRFPProFormaDetailAsync(authenticate, rfpId, rfpSupplierId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add rfp proforma
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="rfpSupplierId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/rfpSupplier/{rfpSupplierId:long}/proforma")]
        [HttpPost]
        public async Task<object> AddProFormaAsync(long rfpId, long rfpSupplierId, [FromBody] AddRFPProFromaDto model)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.RFPProFormaMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.AddRFPSupplierProFormaAsync(authenticate, rfpId, rfpSupplierId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// edit rfp proforma 
        /// </summary>
        /// <param name="proFormaId"></param>
        /// <param name="rfpSupplierId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("RFPSupplier/{rfpSupplierId:long}/proFormaEdit/{proFormaId:long}")]
        [HttpPut]
        public async Task<object> EditRFPProFromaDetailAsync(long rfpSupplierId, long proFormaId, [FromBody] AddRFPProFromaDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPProFormaMng };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.EditRFPSupplierProFormaAsync(authenticate, rfpSupplierId, proFormaId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Delete rfp proforma
        /// </summary>
        /// <param name="rfpSupplierId"></param>
        /// <param name="proFormaId"></param>
        /// <returns></returns>
        [Route("RFPSupplier/{rfpSupplierId:long}/deleteProFroma/{proFormaId:long}")]
        [HttpDelete]
        public async Task<object> DeleteProFroma(long rfpSupplierId, long proFormaId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPProFormaMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, rfpSupplierId);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpService.DeleteProFormaAsync(authenticate, rfpSupplierId, proFormaId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
