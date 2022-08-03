using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.PO.POActivity;
using Raybod.SCM.DataTransferObject.PO.POSupplierDocuemnt;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.ProcurementManagement.Controllers
{
    [Route("api/procurementManagement/PO/{poId:long}/poSupplierDocument")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class POSupplierDocumentController : ControllerBase
    {
        private readonly IPOSupplierDocumentService _poSupplierDocumentService;
        private readonly ILogger<POSupplierDocumentController> _logger;

        public POSupplierDocumentController(
            IPOSupplierDocumentService poSupplierDocumentService,
            ILogger<POSupplierDocumentController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _poSupplierDocumentService = poSupplierDocumentService;
            _logger = logger;
           
               
           
        }

      

        /// <summary>
        /// get po manufacture document list
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetPOSupplierDocumentAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POSupplierDocumentMng, SCMRole.POSupplierDocumentObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poSupplierDocumentService.GetPOSupplierDocumentAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add PO supplier document
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> AddSupplierDocumentAsync(long poId, [FromBody] AddPOSupplierDocumentDto model)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.POSupplierDocumentMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poSupplierDocumentService.AddPOSupplierDocumentAsync(authenticate, poId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// edit PO supplier document
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="poSupplierDocumentId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{poSupplierDocumentId:long}")]
        public async Task<object> EditSupplierDocumentAsync(long poId,long poSupplierDocumentId, [FromBody] EditPOSupplierDocumentDto model)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.POSupplierDocumentMng};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poSupplierDocumentService.EditPOSupplierDocumentAsync(authenticate, poId, poSupplierDocumentId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet]
        [Route("{poSupplierDocumentId:long}/downloadFile")]
        public async Task<object> DownloadPOSupplierDocumentAttachmentAsync(long poId, long poSupplierDocumentId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POSupplierDocumentMng, SCMRole.POSupplierDocumentObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _poSupplierDocumentService.DownloadPOSupplierDocumentAttachmentAsync(authenticate, poId, poSupplierDocumentId, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }
        [HttpDelete]
        [Route("{poSupplierDocumentId:long}")]
        public async Task<object> DeletePOIncpectionAsync(long poId, long poSupplierDocumentId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POSupplierDocumentMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poSupplierDocumentService.DeletePOSupplierDocumentAsync(authenticate, poId, poSupplierDocumentId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
