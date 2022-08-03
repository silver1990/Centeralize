using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Utility.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.ModuleApi.Areas.OperationManagement.Controllers
{
    [Route("api/Document/v1/ContractOperationGroup")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "operationManagement")]
    public class ContractOperationGroupController : ControllerBase
    {
        private readonly IContractOperationGroupService _contractOperationGroupService;
        private readonly ILogger<ContractOperationGroupController> _logger;

        public ContractOperationGroupController(
            IContractOperationGroupService contractOperationGroupService,
             ILogger<ContractOperationGroupController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _contractOperationGroupService = contractOperationGroupService;
            _logger = logger;
           
               
           
        }
        /// <summary>
        /// Get all operation group List 
        /// </summary>        
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetContractOperationGroupListAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> ();

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            authenticate.Roles = new List<string>
            { 
                SCMRole.OperationListGlbObs,
                SCMRole.OperationListObs,
                SCMRole.OperationListMng,
                SCMRole.OperationInProgressGlbObs,
                SCMRole.OperationInProgressMng,
                SCMRole.OperationInProgressObs,
                SCMRole.OperationActivityUpd
            };
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _contractOperationGroupService.GetContractOperationGroupListAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
