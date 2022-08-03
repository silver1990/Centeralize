using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject._PanelOperation.OperationGroup;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.ModuleApi.Areas.OperationManagement.Controllers
{
    [Route("api/Operation/v1/OperationGroup")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "SCMManagement")]
    public class OperationGroupController : ControllerBase
    {

        private readonly IContractOperationGroupService _contractOperationGroupService;
        private readonly ILogger<OperationGroupController> _logger;

        public OperationGroupController(
            IContractOperationGroupService contractOperationGroupService,
            ILogger<OperationGroupController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _contractOperationGroupService = contractOperationGroupService;
            _logger = logger;
           
               
           
        }


        /// <summary>
        /// add new operation group
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> AddOperationGroup([FromBody] AddOperationGroupDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            
            authenticate.Roles = new List<string> {SCMRole.OperationGroupMng };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _contractOperationGroupService.AddOperationGroupAsync(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// edit operation group
        /// </summary>
        /// <param name="operationGroupId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{operationGroupId:int}")]
        public async Task<object> EditOperationGroup(int operationGroupId, [FromBody] AddOperationGroupDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            
            authenticate.Roles = new List<string> { SCMRole.OperationGroupMng };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _contractOperationGroupService.EditOperationGroupAsync(authenticate, operationGroupId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get operation group list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetOperationGroupList()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.OperationGroupObs,SCMRole.OperationGroupMng };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _contractOperationGroupService.GetOperationGroupListAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// delete operation group item
        /// </summary>
        /// <param name="operationGroupId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{operationGroupId:int}")]
        public async Task<object> DeleteOperationGroupAsync(int operationGroupId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            authenticate.Roles = new List<string> { SCMRole.OperationGroupMng };
            var serviceResult = await _contractOperationGroupService.DeleteOperationGroupAsync(authenticate, operationGroupId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
