using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Supplier;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.SCMManagement.Controllers
{
    [Route("api/SCMManagement/v1/Supplier")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "Setting")]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly IProductGroupService _productGroupService;
        private readonly ILogger<SupplierController> _logger;

        public SupplierController(
            ISupplierService supplierService,
            IProductGroupService productGroupService,
            ILogger<SupplierController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _supplierService = supplierService;
            _productGroupService = productGroupService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// Get Product Group list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Route("ProductGroup")]
        [HttpGet]
        public async Task<object> GetProductGroupAsync([FromQuery] string query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.SupplierMng, SCMRole.SupplierObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productGroupService.GetProductGroupAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add new Supplier
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> AddSupplierAsync([FromBody] AddSupplierDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.SupplierMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _supplierService.AddSupplierAsync(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Supplier list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetSupplierAsync([FromQuery] SupplierQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.SupplierMng, SCMRole.SupplierObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _supplierService.GetSupplierAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Edit Supplier item
        /// </summary>
        /// <param name="supplierId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{supplierId:int}")]
        public async Task<object> EditSupplierAsync(int supplierId, [FromBody] AddSupplierDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.SupplierMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _supplierService.EditSupplierAsync(authenticate, supplierId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Delete Supplier item
        /// </summary>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{supplierId:int}")]
        public async Task<object> DeleteSupplierAsync(int supplierId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.SupplierMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _supplierService.DeleteSupplierAsync(authenticate, supplierId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add supplier new user
        /// </summary>
        /// <param name="supplierId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{supplierId:int}/supplierUser")]
        public async Task<object> AddSupplierUserAsync(int supplierId, [FromBody] AddSupplierUserDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.SupplierMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _supplierService.AddSupplierUserAsync(authenticate, supplierId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get supplier user
        /// </summary>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        [HttpGet, Route("{supplierId:int}/supplierUser")]
        public async Task<object> GetSupplierUserAsync(int supplierId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.SupplierMng, SCMRole.SupplierObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _supplierService.GetSupplierUserAsync(authenticate, supplierId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// delete supplier user
        /// </summary>
        /// <param name="supplierId"></param>
        /// <param name="supplierUserId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{supplierId:int}/supplierUser/{supplierUserId:int}")]
        public async Task<object> DeleteSupplierUserByIdAsync(int supplierId, int supplierUserId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.SupplierMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _supplierService.DeleteSupplierUserByIdAsync(authenticate, supplierId, supplierUserId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}