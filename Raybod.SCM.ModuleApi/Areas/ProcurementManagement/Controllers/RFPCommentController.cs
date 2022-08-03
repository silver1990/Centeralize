using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.RFP;
using Raybod.SCM.DataTransferObject.RFP.RFPComment;
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
    [Route("api/procurementManagement/rfp/comment")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class RFPCommentController : ControllerBase
    {
        private readonly IRFPCommentService _rfpCommentService;
        private readonly ILogger<RFPCommentController> _logger;

        public RFPCommentController(IRFPCommentService rfpCommentService,ILogger<RFPCommentController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _rfpCommentService = rfpCommentService;
            _logger = logger;
           
               
           
        }
        /// <summary>
        /// Get RFP proforma comment
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="rfpSupplierId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/rfpSupplier/{rfpSupplierId:long}/proFromaComment")]
        [HttpGet]
        public async Task<object> GetRFPProFormaComment(long rfpId, long rfpSupplierId, [FromQuery] RFPCommentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPProFormaMng, SCMRole.RFPProFromaObs };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpCommentService.GetRFPProFormaCommentAsync(authenticate, rfpId, rfpSupplierId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add rfp proforma comment
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="rfpSupplierId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/rfpSupplier/{rfpSupplierId:long}/proFormaComment")]
        [HttpPost]
        public async Task<object> AddRFPProForma(long rfpId, long rfpSupplierId, [FromBody] AddProFromaCommentDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.RFPProFormaMng, SCMRole.RFPProFromaObs };


            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpCommentService.AddRFPProFormaCommentAsync(authenticate, rfpId, rfpSupplierId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get RFP Tech Comment
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="rfpSupplierId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/rfpSupplier/{rfpSupplierId:long}/techComment")]
        [HttpGet]
        public async Task<object> GetRFPTechComment(long rfpId, long rfpSupplierId, [FromQuery] RFPCommentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPTechEvaluationMng, SCMRole.RFPTechMng, SCMRole.RFPTechObs };

            query.InqueryType = RFPInqueryType.TechnicalInquery;

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpCommentService.GetRFPCommentAsync(authenticate, rfpId, rfpSupplierId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get User Mentions list
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="inqueryType"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/rfpSupplier/comment/userMention")]
        [HttpGet]
        public async Task<object> GetUserMentionsAsync(long rfpId, RFPInqueryType inqueryType)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);

            if (inqueryType == RFPInqueryType.TechnicalInquery)
                authenticate.Roles = new List<string> { SCMRole.RFPTechEvaluationMng, SCMRole.RFPTechObs, SCMRole.RFPTechMng };
            else if (inqueryType == RFPInqueryType.CommercialInquery)
                authenticate.Roles = new List<string> { SCMRole.RFPCommercialMng, SCMRole.RFPCommercialEvaluationMng, SCMRole.RFPCommercialObs };
            else  
                authenticate.Roles = new List<string> { SCMRole.RFPProFormaMng, SCMRole.RFPProFromaObs };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpCommentService.GetUserMentionsAsync(authenticate, rfpId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get RFP Commercial Comment
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="rfpSupplierId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/rfpSupplier/{rfpSupplierId:long}/commercialComment")]
        [HttpGet]
        public async Task<object> GetRFPCommercialComment(long rfpId, long rfpSupplierId, [FromQuery] RFPCommentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPCommercialMng, SCMRole.RFPCommercialEvaluationMng, SCMRole.RFPCommercialObs };

            query.InqueryType = RFPInqueryType.CommercialInquery;

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpCommentService.GetRFPCommentAsync(authenticate, rfpId, rfpSupplierId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add RFP Tech Comment
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="rfpSupplierId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/rfpSupplier/{rfpSupplierId:long}/techComment")]
        [HttpPost]
        public async Task<object> AddRFPTechComment(long rfpId, long rfpSupplierId, [FromBody] AddRFPCommentDto model)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.RFPTechEvaluationMng, SCMRole.RFPTechMng, SCMRole.RFPTechObs };

            model.InqueryType = RFPInqueryType.TechnicalInquery;

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpCommentService.AddRFPCommentAsync(authenticate, rfpId, rfpSupplierId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add RFP Commercial Comment
        /// </summary>
        /// <param name="rfpId"></param>
        /// <param name="rfpSupplierId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("{rfpId:long}/rfpSupplier/{rfpSupplierId:long}/commercialComment")]
        [HttpPost]
        public async Task<object> AddRFPCommercialComment(long rfpId, long rfpSupplierId, [FromBody] AddRFPCommentDto model)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }
            model.InqueryType = RFPInqueryType.CommercialInquery;


            authenticate.Roles = new List<string> { SCMRole.RFPCommercialMng, SCMRole.RFPCommercialEvaluationMng, SCMRole.RFPCommercialObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _rfpCommentService.AddRFPCommentAsync(authenticate, rfpId, rfpSupplierId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet, Route("{rfpId:long}/rfpSupplier/{rfpSupplierId:long}/commentAttachment/{commentId:long}/downloadFile")]
        public async Task<object> DownloadTechCommentAttachmentAsync(long rfpId, long rfpSupplierId, long commentId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RFPTechEvaluationMng, SCMRole.RFPTechMng, SCMRole.RFPTechObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _rfpCommentService.DownloadRFPCommentAttachmentAsync(authenticate, rfpId, rfpSupplierId, commentId, fileSrc);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, streamResult.ContentType,streamResult.FileName);

        }

        
    }
}
