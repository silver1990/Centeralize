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
    [Route("api/Document/v1/DocumentGroup")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "documentManagement")]
    public class DocumentGroupController : ControllerBase
    {
        private readonly IContractDocumentGroupService _contractDocumentGroupService;
        private readonly ILogger<DocumentGroupController> _logger;

        public DocumentGroupController(
            IContractDocumentGroupService contractDocumentGroupService,
            ILogger<DocumentGroupController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _contractDocumentGroupService = contractDocumentGroupService;
            _logger=logger;
           
            
               
           
        }

        /// <summary>
        /// add new document group
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> AddDocumentGroup([FromBody] AddDocumentGroupDto model)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.DocumentGroupMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _contractDocumentGroupService.AddDocumentGroupAsync(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// edit document group
        /// </summary>
        /// <param name="documentGroupId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{documentGroupId:int}")]
        public async Task<object> EditDocumentGroup(int documentGroupId, [FromBody] AddDocumentGroupDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.DocumentGroupMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _contractDocumentGroupService.EditDocumentGroupAsync(authenticate, documentGroupId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get document group list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetDocumentGroupList()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.DocumentGroupMng, SCMRole.DocumentGroupObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _contractDocumentGroupService.GetDocumentGroupListAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        
        /// <summary>
        /// delete document group item
        /// </summary>
        /// <param name="documentGroupId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{documentGroupId:int}")]
        public async Task<object> DeleteDocumentGroupAsync(int documentGroupId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.DocumentGroupMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _contractDocumentGroupService.DeleteDocumentGroupAsync(authenticate, documentGroupId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet, Route("list")]
        public async Task<object> GetContractDocumentGroupListAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.DocumentMng, SCMRole.DocumentObs, SCMRole.DocumentGlbObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _contractDocumentGroupService.GetContractDocumentGroupListAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}