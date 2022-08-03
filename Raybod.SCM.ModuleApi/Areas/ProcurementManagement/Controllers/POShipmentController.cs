using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.PurchaseManagement.Controllers
{
    [Route("api/procurementManagement/PO/{poId:long}/Shipment")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class POShipmentController : ControllerBase
    {

        private readonly IPackService _packService;
        private readonly ILogisticService _logisticService;
        private readonly ILogger<POShipmentController> _logger;

        public POShipmentController(
            IPackService packService,
            ILogisticService logisticService,
            ILogger<POShipmentController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _packService = packService;
            _logisticService = logisticService;
            _logger = logger;
           
               
           
        }

        ///// <summary>
        ///// Get Po Pack Logistic list
        ///// </summary>
        ///// <param name="poId"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<object> GetPOLogisticAsync(long poId)
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { SCMRole.LogisticMng };

        //    var serviceResult = await _logisticService.GetPoPackLogisticAsync(authenticate, poId);
        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}

        ///// <summary>
        ///// get pack details
        ///// </summary>
        ///// <param name="poId"></param>
        ///// <param name="packId"></param>
        ///// <returns></returns>
        //[HttpGet, Route("{packId:long}/packDetails")]
        //public async Task<object> GetPackDetailsByPackIdAsync(long poId, long packId)
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { SCMRole.LogisticMng };

        //    var serviceResult = await _packService
        //        .GetPackByIdAsync(authenticate, poId, packId);
        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}

        /// <summary>
        /// get logistic
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <returns></returns>
        [HttpGet, Route("{packId:long}")]
        public async Task<object> GetPackLogisticByPackIdAsync(long poId, long packId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs, SCMRole.LogisticMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _logisticService
                .GetPackLogisticByPackIdAsync(authenticate, poId, packId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Start Transportation
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        [HttpPost, Route("{packId:long}/startTransportation")]
        public async Task<object> StratTransportation(long poId, long packId, LogisticStep step)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.LogisticMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _logisticService
                .StartTransportationAsync(authenticate, poId, packId, step);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Completed Transportation
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        [HttpPost, Route("{packId:long}/completedTransportation")]
        public async Task<object> CompletedTransportation(long poId, long packId, LogisticStep step)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.LogisticMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _logisticService
                .CompeleteTransportationAsync(authenticate, poId, packId, step);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Start ClearancePort
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        [HttpPost, Route("{packId:long}/startClearancePort")]
        public async Task<object> StartClearancePortAsync(long poId, long packId, LogisticStep step)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.LogisticMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _logisticService
                .StartClearancePortAsync(authenticate, poId, packId, step);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Compelete ClearancePort
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        [HttpPost, Route("{packId:long}/compeleteClearancePort")]
        public async Task<object> CompeleteClearancePortAsync(long poId, long packId, LogisticStep step)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.LogisticMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _logisticService
                .CompeleteClearancePortAsync(authenticate, poId, packId, step);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Logistic Attachment list
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        [HttpGet, Route("{packId:long}/Attachment")]
        public async Task<object> GetLogisticAttachmentAsync(long poId, long packId, LogisticStep step)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs, SCMRole.LogisticMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _logisticService.GetLogisticAttachmentAsync(authenticate, poId, packId, step);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add Logistic Attachment 
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <param name="step"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost, Route("{packId:long}/AddAttachment")]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<object> AddLogisticAttachmentAsync(long poId, long packId, LogisticStep step)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.LogisticMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var files = HttpContext.Request.Form.Files;
            var serviceResult = await _logisticService.AddLogisticAttachmentAsync(authenticate, poId, packId, step, files);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download Logistic Attachment 
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <param name="step"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet, Route("{packId:long}/attachment/download")]
        public async Task<object> DownloadLogisticAttachmentAsync(long poId, long packId, LogisticStep step, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs, SCMRole.LogisticMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _logisticService.DownloadLogisticAttachmentAsync(authenticate, poId, packId, step, fileSrc);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, "application/octet-stream");

        }

        /// <summary>
        /// Delete Logistic Attachment
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <param name="step"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpPost, Route("{packId:long}/deleteAttachment")]
        public async Task<object> DeleteLogisticAttachmentAsync(long poId, long packId, LogisticStep step, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.LogisticMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _logisticService.DeleteLogisticAttachmentAsync(authenticate, poId, packId, step, fileSrc);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

    }
}