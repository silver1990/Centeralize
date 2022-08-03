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

namespace Raybod.SCM.ModuleApi.Areas.Warehouse.Controllers
{
    [Route("api/procurementManagement/warehouse")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
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
        /// Get Warehouse Products
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetWarehouseProductAsync([FromQuery] WarehouseProductQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseInventoryObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _warehouseService.GetWarehouseProductAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Warehouse Product Logs
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("excelExportProductLogs/{productId:int}")]
        public async Task<object> GetWarehouseProductLogsAsync(int productId, [FromQuery] WarehouseProductLogQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseInventoryObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _warehouseService.GetWarehouseProductLogsAsync(authenticate, productId, query);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }

        /// <summary>
        /// Export Excel Warehouse Product
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("ExportExcelProducts")]
        public async Task<object> ExportExcelWarehouseProductAsync([FromQuery] WarehouseProductQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.WarehouseInventoryObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var streamResult = await _warehouseService.ExportExcelWarehouseProductAsync(authenticate, query);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }
    }
}