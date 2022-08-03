using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Warehouse;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.SCMManagement.Controllers
{
    [Route("api/SCMManagement/v1/warehouse")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "Setting")]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ILogger<WarehouseController> _logger;

        public WarehouseController(
            IWarehouseService warehouseService,
            ILogger<WarehouseController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _warehouseService = warehouseService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// Get Warehouse List 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetWarehouseListAsync([FromQuery] WarehouseQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseQCMng, SCMRole.WarehouseObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.GetWarehouseListAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add Warehouse 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> AddWarehouseAsync([FromBody] AddWarehouseDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.AddWarehouseAsync(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Warehouse By warehouseId 
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <returns></returns>
        [HttpGet, Route("{warehouseId:int}")]
        public async Task<object> GetWarehouseByIdAsync(int warehouseId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseQCMng, SCMRole.WarehouseObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.GetWarehouseByIdAsync(authenticate, warehouseId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Edit Warehouse item
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{warehouseId:int}")]
        public async Task<object> EditWarehouseAsync(int warehouseId, [FromBody] AddWarehouseDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.EditWarehouseAsync(authenticate, warehouseId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Remove Warehouse By warehouseId 
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{warehouseId:int}")]
        public async Task<object> RemoveWarehouseAsync(int warehouseId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.RemoveWarehouseAsync(authenticate, warehouseId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        ///// <summary>
        ///// Get Warehouse Products
        ///// </summary>
        ///// <param name="query"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<object> GetWarehouseProducts([FromQuery] WarehouseProductQueryDto query)
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseQCMng, SCMRole.WarehouseObs };

        //    var serviceResult = await _warehouseService.GetWarehouseProductAsync(authenticate, query);
        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}

        ///// <summary>
        ///// Get Warehouse Product Logs
        ///// </summary>
        ///// <param name="productId"></param>
        ///// <param name="query"></param>
        ///// <returns></returns>
        //[HttpGet, Route("productLogs/{productId:int}")]
        //public async Task<object> GetProductLogs(int productId, [FromQuery] WarehouseProductLogQueryDto query)
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseQCMng, SCMRole.WarehouseObs };

        //    var serviceResult = await _warehouseService.GetWarehouseProductLogsAsync(authenticate, productId, query);
        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}

        ///// <summary>
        ///// Export Excel Warehouse Product
        ///// </summary>
        ///// <param name="query"></param>
        ///// <returns></returns>
        //[HttpGet, Route("ExportExcelProducts")]
        //public async Task<object> ExportWarehouseProductAsync([FromQuery] WarehouseProductQueryDto query)
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    authenticate.Roles = new List<string> { SCMRole.WarehouseMng, SCMRole.WarehouseQCMng, SCMRole.WarehouseObs };

        //    var streamResult = await _warehouseService.ExportExcelWarehouseProductAsync(authenticate, query);
        //    if (streamResult == null)
        //        return NotFound();

        //    return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        //}
    }
}