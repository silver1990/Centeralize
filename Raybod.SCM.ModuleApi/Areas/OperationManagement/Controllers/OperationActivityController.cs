using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject._PanelOperation;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.ModuleApi.Areas.OperationManagement.Controllers
{
    [Route("api/Operation/v1/Operation")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "operationManagement")]
    public class OperationActivityController : ControllerBase
    {
        private readonly IOperationActivityService _operationActivityService;
        private readonly ILogger<OperationActivityController> _logger;

        public OperationActivityController(
            IOperationActivityService operationActivityService,
            ILogger<OperationActivityController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _operationActivityService = operationActivityService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// get activity list
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{operationId:Guid}/ActivityList")]
        public async Task<object> GetActivityListAsync(Guid operationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationActivityService.GetOperationActivityListAsync(authenticate, operationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get activity user list
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{operationId:Guid}/ActivityUser")]
        public async Task<object> GetActivityUserListAsync(Guid operationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {SCMRole.OperationInProgressMng,SCMRole.OperationActivityUpd };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationActivityService.GetActivityUserListAsync(authenticate, operationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add operation activity
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{operationId=Guid}/AddActivity")]
        public async Task<object> AddOperationActivityAsync(Guid operationId, [FromBody] AddOperationActivityDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.OperationInProgressMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationActivityService.AddOperationActivityAsync(authenticate, operationId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// delete operation activity
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="operationActivityId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{operationId:Guid}/DeleteActivity/{operationActivityId:long}")]
        public async Task<object> DeleteOperationActivityAsync(Guid operationId, long operationActivityId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.OperationInProgressMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationActivityService.DeleteOperationActivityAsync(authenticate, operationId,operationActivityId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Set operation activity status 
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="operationActivityId"></param>
        /// <returns></returns>
        [HttpPost, Route("{operationId:Guid}/Activity/{operationActivityId:long}/changeStatus")]
        public async Task<object> SetOperaiontActivityStatusAsync(Guid operationId, long operationActivityId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.OperationInProgressMng,SCMRole.OperationActivityUpd };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationActivityService.SetOperationActivityStatusAsync(authenticate, operationId, operationActivityId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Edit operation activity
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="operationActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{operationId:Guid}/EditActivity/{operationActivityId:long}")]
        public async Task<object> EditOperationActivityAsync(Guid operationId, long operationActivityId, [FromBody] AddOperationActivityDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.OperationInProgressMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationActivityService.EditOperationActivityAsync(authenticate, operationId,operationActivityId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add activity timeSheet 
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="operationActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{operationId:Guid}/Activity/{operationActivityId:long}/AddTimeSheet")]
        public async Task<object> AddActivityTimeSheetAsync(Guid operationId, long operationActivityId, [FromBody] AddActivityTimeSheetDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.OperationInProgressMng,SCMRole.OperationActivityUpd };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationActivityService.AddActivityTimeSheetAsync(authenticate, operationId, operationActivityId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get activity timeSheet 
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="operationActivityId"></param>
        /// <returns></returns>
        [HttpGet, Route("{operationId:Guid}/Activity/{operationActivityId:long}/GetTimeSheets")]
        public async Task<object> GetActivityTimeSheetAsync(Guid operationId,long operationActivityId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationActivityService.GetActivityTimeSheetAsync(authenticate, operationId, operationActivityId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Delete activity timeSheet 
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="operationActivityId"></param>
        /// <param name="activityTimeSheetId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{operationId:Guid}/Activity/{operationActivityId:long}/DeleteTimeSheet/{activityTimeSheetId:long}")]
        public async Task<object> DeleteActivityTimeSheetAsync(Guid operationId,  long operationActivityId, long activityTimeSheetId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.OperationInProgressMng, SCMRole.OperationActivityUpd };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationActivityService.DeleteActivityTimeSheetAsync(authenticate, operationId, operationActivityId, activityTimeSheetId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
