using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.PO.POActivity;
using Raybod.SCM.DataTransferObject.PO.POInspection;
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
    [Route("api/procurementManagement/PO/{poId:long}/poInspection")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class POInspectionController : ControllerBase
    {
        private readonly IPOInspectionService _poInspectionService;
        private readonly ILogger<POInspectionController> _logger;

        public POInspectionController(
            IPOInspectionService poInspectionService,
            ILogger<POInspectionController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _poInspectionService = poInspectionService;
            _logger = logger;
           
               
           
        }

      
        /// <summary>
        /// get po inspection list
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetPoInspectionListAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {SCMRole.POInspectionObs,SCMRole.POInspectionMng};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poInspectionService.GetPoInspectionAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// get inspection user list
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("InspectionUser")]
        public async Task<object> GetInspectionUserListAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {  SCMRole.POInspectionMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poInspectionService.GetInspectionUserListAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add PO inspection
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> AddPOInspectionAsync(long poId, [FromBody] AddPOInspectionDto model)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.POInspectionMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poInspectionService.AddPoInspectionAsync(authenticate, poId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add inspection result 
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="inspectionId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{inspectionId:long}/result")]
        public async Task<object> AddInspectionResultAsync(long poId, long inspectionId, [FromBody] AddPOInspectionResultDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.POInspectionMng };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poInspectionService.AddInspectionResultAsync(authenticate, poId, inspectionId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet]
        [Route("{incpectionId:long}/downloadFile")]
        public async Task<object> DownloadPOIncpectionAttachmentAsync(long poId, long incpectionId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POInspectionMng,SCMRole.POInspectionObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _poInspectionService.DownloadPOInspectionAttachmentAsync(authenticate, poId, incpectionId, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }
        [HttpDelete]
        [Route("{incpectionId:long}")]
        public async Task<object> DeletePOIncpectionAsync(long poId, long incpectionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POInspectionMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _poInspectionService.DeletePoInspectionAsync(authenticate, poId, incpectionId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
