using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Contract;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.Sale.Controllers
{
    [Route("api/sale/v1/contract")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "sale")]
    public class ContractFormConfigController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IContractFormConfigService _contractFormConfigService;
        private readonly ILogger<ContractFormConfigController> _logger;

        public ContractFormConfigController(
            IContractService contractService,
            IContractFormConfigService contractFormConfigService,
            ILogger<ContractFormConfigController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _contractService = contractService;
            _contractFormConfigService = contractFormConfigService;
            _logger = logger;
           
               
           
        }


        /// <summary>
        /// get contract formConfig list
        /// </summary>
        /// <param name="contractCode"></param>
        /// <returns></returns>
        [HttpGet, Route("{contractCode}/FormConfig")]
        public async Task<object> GetContractFormConfigListAsync(string contractCode)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng, SCMRole.ContractReg, SCMRole.ContractObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, contractCode);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _contractFormConfigService.GetContractFormConfigListAsync(authenticate, contractCode);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Edit FormConfig item 
        /// </summary>
        /// <param name="contractCode"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{contractCode}/FormConfig")]
        public async Task<object> EditFormConfigListAsync(string contractCode, [FromBody] ContractFormConfigDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng, SCMRole.ContractReg };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, contractCode);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _contractFormConfigService.EditFormConfigListAsync(authenticate, contractCode, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Is Valid Current FixedPart
        /// </summary>
        /// <param name="contractCode"></param>
        /// <param name="fixedPart"></param>
        /// <returns></returns>
        [HttpGet, Route("{contractCode}/IsValidFixedPart/{fixedPart}")]
        public async Task<object> IsValidCurrentFixedPart(string contractCode, string fixedPart)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.ContractMng, SCMRole.ContractReg };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, contractCode);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _contractFormConfigService.IsValidCurrentFixedPart(authenticate, contractCode, fixedPart);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

    }
}