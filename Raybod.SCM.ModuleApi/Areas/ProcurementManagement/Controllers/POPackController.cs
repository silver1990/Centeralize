using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Packing;
using Raybod.SCM.DataTransferObject.QualityControl;
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
    [Route("api/procurementManagement/PO/{poId:long}/pack")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class POPackController : ControllerBase
    {
        private readonly IPackService _packService;
        private readonly IReceiptService _warehouseReceiptService;
        private readonly ILogger<POPackController> _logger;

        public POPackController(
            IPackService packService,
            ILogger<POPackController> logger,IHttpContextAccessor httpContextAccessor,
            IReceiptService warehouseReceiptService)
        {
            _packService = packService;
            _logger = logger;
            _warehouseReceiptService = warehouseReceiptService;
           
               
           
        }

        /// <summary>
        /// Get Waiting PoSubject list For Packing 
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpGet, Route("waitingForPacking")]
        public async Task<object> GetWaitingPoSubjectForAddPackingAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs, SCMRole.PackMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _packService
                .GetWaitingPoSubjectForAddPackingAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        ///  add new pack
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> AddPackAsync(long poId, [FromBody] AddPackDto model)
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

            
            authenticate.Roles = new List<string> { SCMRole.PackMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _packService.AddPackAsync(authenticate, poId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get pack list
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetPackListAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs, SCMRole.PackMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _packService
                .GetPackListAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get pack details 
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <returns></returns>
        [HttpGet, Route("{packId:long}")]
        public async Task<object> GetPackByIdAsync(long poId, long packId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs, SCMRole.PackMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _packService
                .GetPackByIdAsync(authenticate, poId, packId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get pack for edit
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <returns></returns>
        [HttpGet, Route("{packId:long}/ForEdit")]
        public async Task<object> GetPackForEditById(long poId, long packId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs, SCMRole.PackMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _packService
                .GetPackByIdForEditAsync(authenticate, poId, packId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add quality control for pack
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{packId:long}/QualityControl")]
        public async Task<object> AddQulityControlAsync(long poId, long packId, [FromBody] AddQualityControlDto model)
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

           
            authenticate.Roles = new List<string> { SCMRole.PackQCMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _packService.AddQulityControlAsync(authenticate, poId, packId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
       
        /// <summary>
        /// get quality control of pack
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <returns></returns>
        [HttpGet, Route("{packId:long}/QualityControl")]
        public async Task<object> GetQualityControlByPackIdAsync(long poId, long packId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs, SCMRole.PackQCMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _packService
                .GetQualityControlByPackIdAsync(authenticate, poId, packId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// edit pack
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{packId:long}")]
        public async Task<object> EditPackAsync(long poId, long packId, [FromBody] List<AddPackSubjectDto> model)
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

           
            authenticate.Roles = new List<string> { SCMRole.PackMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _packService.EditPackAsync(authenticate, poId, packId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// delete pack attachment
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpPost, Route("{packId:long}/DeleteAttachment")]
        public async Task<object> DeletePackAttachmentAsync(long poId, long packId, string fileName)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PackMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileName);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _packService.DeletePackAttachmentAsync(authenticate, poId, packId, fileName);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add pack attachment
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost, Route("{packId:long}/AddAttachment")]
        public async Task<object> AddPackAttachmentAsync(long poId, long packId, [FromBody] AddAttachmentDto file)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PackQCMng, SCMRole.PackMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, file);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _packService.AddPackAttachmentAsync(authenticate, poId, packId, file);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// delete pack
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{packId:long}/Delete")]
        public async Task<object> DeletePack(long poId, long packId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PackMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _packService
                .DeletePackAsync(authenticate, poId, packId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download Pack Attachment 
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet, Route("{packId:long}/attachment/download")]
        public async Task<object> DownloadPackAttachmentAsync(long poId, long packId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PackQCMng, SCMRole.PackMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _packService.DownloadPackAttachmentAsync(authenticate, poId, packId, fileSrc);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, "application/octet-stream");

        }

        /// <summary>
        /// Download pack QualityControl attachment
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="packId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet, Route("{packId:long}/QCAttachment/download")]
        public async Task<object> DownloadQualityControlFile(long poId, long packId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs, SCMRole.PackQCMng, SCMRole.PackMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _packService.DownloadPackQualityControlAttachmentAsync(authenticate, poId, packId, fileSrc);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, "application/octet-stream");

        }
        [HttpGet, Route("reportDeliveryProducts/{packId:long}")]
        public async Task<object> GetReportReceiptProductByPackIdAsync(long poId, long packId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POMng, SCMRole.POObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseReceiptService.GetReportReceiptProductByPackIdAsync(authenticate, poId, packId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}