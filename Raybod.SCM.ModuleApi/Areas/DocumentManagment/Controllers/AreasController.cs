using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Services.Core.DocumentManagement;
using Raybod.SCM.Utility.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Raybod.SCM.ModuleApi.Areas.DocumentManagment.Controllers
{
    [Route("api/Document/v1")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "documentManagement")]
    public class AreasController : ControllerBase
    {
        private readonly IAreaServices _areaService;
        private readonly ILogger<AreasController> _logger;

        public AreasController(IAreaServices areaService, ILogger<AreasController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _areaService = areaService;
            _logger = logger;
          
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("areas")]
        public async Task<object> GetUserAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _areaService.GetAreaListAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("addarea")]
        public async Task<object> AddAreaAsync([FromBody] AreaAddDTO model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _areaService.AddAreaAsync(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Edit area 
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{areaId:int}")]
        public async Task<object> EditSupplierAsync(int areaId, [FromBody] AreaAddDTO model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _areaService.EditAreaByAreaIdAsync(authenticate, areaId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Delete area
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{areaId:int}")]
        public async Task<object> DeleteSupplierAsync(int areaId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _areaService.DeleteAreaAsync(authenticate, areaId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

    }
}
