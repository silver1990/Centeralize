using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.PO.POActivity;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.DocumentManagment.Controllers
{
    [Route("api/procurementManagement/PO/{poId:long}/POActivity")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "procurementManagement")]
    public class POActivityController : ControllerBase
    {
        private readonly IPOActivityService _poActivityService;
        private readonly ILogger<POActivityController> _logger;

        public POActivityController(
            IPOActivityService poActivityService,
            ILogger<POActivityController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _poActivityService = poActivityService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// get activity user list
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ActivityUser")]
        public async Task<object> GetActivityUserListAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.POObs,
                SCMRole.POMng,
                SCMRole.POActivityMng,
                SCMRole.POActivityOwner
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poActivityService.GetActivityUserListAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add PO activity
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> AddPOActivityAsync(long poId, [FromBody] AddPOActivityDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.POActivityMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poActivityService.AddPOActivityAsync(authenticate, poId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get po activity list
        /// </summary>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetPOActivityListAsync(long poId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.POMng,
                SCMRole.POObs,
                SCMRole.POActivityMng,
                SCMRole.POActivityOwner,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poActivityService.GetPOActivityListAsync(authenticate, poId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// delete PO activity
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="poActivityId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{poActivityId:long}")]
        public async Task<object> DeletePOActivityAsync(long poId, long poActivityId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POActivityMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poActivityService.DeletePOActivityAsync(authenticate, poId, poActivityId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Set PO Activity Status 
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="poActivityId"></param>
        /// <returns></returns>
        [HttpPost, Route("{poActivityId:long}/changeStatus")]
        public async Task<object> SetPOActivityStatusAsync(long poId, long poActivityId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POActivityMng, SCMRole.POActivityOwner };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poActivityService.SetActivityStatusAsync(authenticate, poId, poActivityId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Edit PO Activity
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="poActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{poActivityId:long}")]
        public async Task<object> EditPOActivityAsync(long poId, long poActivityId, [FromBody] AddPOActivityDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.POActivityMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poActivityService.EditPOActivityAsync(authenticate, poId, poActivityId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add Activity TimeSheet 
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="poActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{poActivityId:long}/TimeSheet")]
        public async Task<object> AddActivityTimeSheetAsync(long poId, long poActivityId, [FromBody] AddActivityTimeSheetDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.POActivityOwner, SCMRole.POActivityMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poActivityService.AddActivityTimeSheetAsync(authenticate, poId, poActivityId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Activity TimeSheet 
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="poActivityId"></param>
        /// <returns></returns>
        [HttpGet, Route("{poActivityId:long}/TimeSheet")]
        public async Task<object> GetActivityTimeSheetAsync(long poId, long poActivityId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.POMng,
                SCMRole.POObs,
                SCMRole.POActivityMng,
                SCMRole.POActivityOwner,
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poActivityService.GetActivityTimeSheetAsync(authenticate, poId, poActivityId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Delete Activity TimeSheet 
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="poActivityId"></param>
        /// <param name="activityTimeSheetId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{poActivityId:long}/TimeSheet/{activityTimeSheetId:long}")]
        public async Task<object> DeleteActivityTimeSheetAsync(long poId, long poActivityId, long activityTimeSheetId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>
            {
                SCMRole.POActivityOwner,
                SCMRole.POActivityMng
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _poActivityService.DeleteActivityTimeSheetAsync(authenticate, poId, poActivityId, activityTimeSheetId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


    }
}