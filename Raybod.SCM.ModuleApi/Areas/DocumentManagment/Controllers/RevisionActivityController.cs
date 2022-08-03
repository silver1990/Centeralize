using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Document;
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
    [Route("api/Document/v1/Document/{documentId:long}")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "documentManagement")]
    public class RevisionActivityController : ControllerBase
    {
        private readonly IRevisionActivityService _revisionActivityService;
        private readonly ILogger<RevisionActivityController> _logger;

        public RevisionActivityController(
            IRevisionActivityService revisionActivityService,
            ILogger<RevisionActivityController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _revisionActivityService = revisionActivityService;
            _logger=logger;
           
            
                
           
           
        }

        /// <summary>
        /// get activity user list
        /// </summary>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DocumentRevision/{revisionId:long}/ActivityUser")]
        public async Task<object> GetActivityUserListAsync(long revisionId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionObs,
                SCMRole.RevisionGlbObs,
                SCMRole.RevisionActivityMng
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionActivityService.GetActivityUserListAsync(authenticate, revisionId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add revision activity
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="documentRevisionId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("DocumentRevision/{documentRevisionId:long}/Activity")]
        public async Task<object> AddRevisionActivityAsync(long documentId, long documentRevisionId, [FromBody] AddRevisionActivityDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.RevisionMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionActivityService.AddRevisionActivityAsync(authenticate, documentId, documentRevisionId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// delete revision activity
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="documentRevisionId"></param>
        /// <param name="revisionActivityId"></param>
        /// <returns></returns>
        [HttpDelete, Route("DocumentRevision/{documentRevisionId:long}/Activity/{revisionActivityId:long}")]
        public async Task<object> DeleteRevisionActivityAsync(long documentId, long documentRevisionId, long revisionActivityId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RevisionMng };
         
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionActivityService.DeleteRevisionActivityAsync(authenticate, documentId, documentRevisionId, revisionActivityId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Set Revision Activity Status 
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="documentRevisionId"></param>
        /// <param name="revisionActivityId"></param>
        /// <returns></returns>
        [HttpPost, Route("DocumentRevision/{documentRevisionId:long}/Activity/{revisionActivityId:long}/changeStatus")]
        public async Task<object> SetRevisionActivityStatusAsync(long documentId, long documentRevisionId, long revisionActivityId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RevisionActivityMng, SCMRole.RevisionMng };
           
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionActivityService.SetRevisionActivityStatusAsync(authenticate, documentId, documentRevisionId, revisionActivityId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Edit Revision Activity
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="documentRevisionId"></param>
        /// <param name="revisionActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("DocumentRevision/{documentRevisionId:long}/Activity/{revisionActivityId:long}")]
        public async Task<object> EditRevisionActivityAsync(long documentId, long documentRevisionId, long revisionActivityId, [FromBody] AddRevisionActivityDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.RevisionMng };
           
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionActivityService.EditRevisionActivityAsync(authenticate, documentId, documentRevisionId, revisionActivityId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add Activity TimeSheet 
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="documentRevisionId"></param>
        /// <param name="revisionActivityId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("DocumentRevision/{documentRevisionId:long}/Activity/{revisionActivityId:long}/TimeSheet")]
        public async Task<object> AddActivityTimeSheetAsync(long documentId, long documentRevisionId, long revisionActivityId, [FromBody] AddActivityTimeSheetDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.RevisionMng, SCMRole.RevisionActivityMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionActivityService.AddActivityTimeSheetAsync(authenticate, documentId, documentRevisionId, revisionActivityId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get Activity TimeSheet 
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="documentRevisionId"></param>
        /// <param name="revisionActivityId"></param>
        /// <returns></returns>
        [HttpGet, Route("DocumentRevision/{documentRevisionId:long}/Activity/{revisionActivityId:long}/TimeSheet")]
        public async Task<object> GetActivityTimeSheetAsync(long documentId, long documentRevisionId, long revisionActivityId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionObs,
                SCMRole.RevisionGlbObs,
                SCMRole.RevisionActivityMng
            };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionActivityService.GetActivityTimeSheetAsync(authenticate, documentId, documentRevisionId, revisionActivityId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Delete Activity TimeSheet 
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="documentRevisionId"></param>
        /// <param name="revisionActivityId"></param>
        /// <param name="activityTimeSheetId"></param>
        /// <returns></returns>
        [HttpDelete, Route("DocumentRevision/{documentRevisionId:long}/Activity/{revisionActivityId:long}/TimeSheet/{activityTimeSheetId:long}")]
        public async Task<object> DeleteActivityTimeSheetAsync(long documentId, long documentRevisionId, long revisionActivityId, long activityTimeSheetId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.RevisionMng,
                SCMRole.RevisionActivityMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _revisionActivityService.DeleteActivityTimeSheetAsync(authenticate, documentId, documentRevisionId, revisionActivityId, activityTimeSheetId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


    }
}