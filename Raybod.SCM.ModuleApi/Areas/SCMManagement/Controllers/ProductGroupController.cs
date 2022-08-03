using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.ProductGroup;
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
    [Route("api/SCMManagement/v1/ProductGroup")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "Setting")]
    public class ProductGroupController : ControllerBase
    {
        private readonly IProductGroupService _productGroupService;
        private readonly ILogger<ProductGroupController> _logger;

        public ProductGroupController(
            IProductGroupService productGroupService,
            ILogger<ProductGroupController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _productGroupService = productGroupService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// Add new ProductGroup
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> AddProductGroupAsync([FromBody] AddProductGroupDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.ProductMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productGroupService.AddProductGroupAsync(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get ProductGroup list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetProductGroupAsync([FromQuery] ProductGroupQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ProductMng, SCMRole.ProductObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productGroupService.GetProductGroupAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get ProductGroup Item
        /// </summary>
        /// <param name="productGroupId"></param>
        /// <returns></returns>
        [HttpGet, Route("{productGroupId:int}")]
        public async Task<object> GetProductGroupByIdAsync(int productGroupId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ProductMng, SCMRole.ProductObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productGroupService.GetProductGroupByIdAsync(authenticate, productGroupId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Edit ProductGroup item
        /// </summary>
        /// <param name="productGroupId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{productGroupId:int}")]
        public async Task<object> EditProductGroupAsync(int productGroupId, [FromBody] AddProductGroupDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.ProductMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productGroupService.EditProductGroupByIdAsync(authenticate, productGroupId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Delete ProductGroup item
        /// </summary>
        /// <param name="productGroupId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{productGroupId:int}")]
        public async Task<object> DeleteProductGroupAsync(int productGroupId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ProductMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productGroupService.DeleteProductGroupAsync(authenticate, productGroupId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        
    }
}