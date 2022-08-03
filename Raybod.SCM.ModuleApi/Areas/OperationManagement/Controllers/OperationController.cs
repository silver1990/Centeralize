using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Operation;
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
    [Route("api/Operation/v1/Operation")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "operationManagement")]
    public class OperationController : ControllerBase
    {
        private readonly IOperationService _operationService;
        private readonly ILogger<OperationController> _logger;

        public OperationController(
            IOperationService operationService,
            ILogger<OperationController> logger,IHttpContextAccessor httpContextAccessor
            )
        {
            _operationService = operationService;
            _logger = logger;
           
               
           
        }

        /// <summary>
        /// Add operation list 
        /// </summary>
        /// <param name="operationGroupId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("operationGroup/{operationGroupId:int}")]
        public async Task<object> AddOperationAsync(int operationGroupId, [FromBody] List<AddOperationDto> model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

           
            authenticate.Roles = new List<string> { SCMRole.OperationListMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.AddOperationAsync(authenticate, operationGroupId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// add single operation
        /// </summary>
        /// <param name="operationGroupId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("opeartionGroup/{operationGroupId:int}/addSingle")]
        public async Task<object> AddOperationAsync(int operationGroupId, [FromBody] AddOperationDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            
            authenticate.Roles = new List<string> { SCMRole.OperationListMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.AddOperationAsync(authenticate, operationGroupId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get operation list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetOperationsAsync([FromQuery] OperationQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>
            {
                SCMRole.OperationListMng,
                SCMRole.OperationListGlbObs,
                SCMRole.OperationListObs
            };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.GetOperationAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get operation
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        [HttpGet("{operationId:Guid}")]
        public async Task<object> GetOperationsAsync(Guid operationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.GetOperationByIdAsync(authenticate, operationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get started operation list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("getStartedOperation")]
        public async Task<object> GetStartedOperationsAsync([FromQuery] OperationQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>
            {
                SCMRole.OperationInProgressGlbObs,
                SCMRole.OperationInProgressMng,
                SCMRole.OperationInProgressObs,
                SCMRole.OperationActivityUpd
            };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.GetStartedOperationAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get not started operation list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("getNotStartedOperation")]
        public async Task<object> GetNotStartedOperationsAsync([FromQuery] OperationQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {SCMRole.OperationInProgressMng };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.GetNotStartedOperationAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Edit operation 
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{operationId:Guid}")]
        public async Task<object> EditOperatinAsync(Guid operationId, [FromBody] EditOperationDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            
            authenticate.Roles = new List<string> { SCMRole.OperationListMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.EditOperationByOperationIdAsync(authenticate, operationId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Active or deactive operation 
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        [HttpPut, Route("ActiveDeactive/{operationId:Guid}")]
        public async Task<object> ActiveOrDeactiveOperatinAsync(Guid operationId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.OperationListMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.ActiveOrDeactiveOperationByOperationIdAsync(authenticate, operationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Start single operation 
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("startOperation/{operationId:Guid}")]
        public async Task<object> StartOpearation(Guid operationId, StartOperationDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.OperationInProgressMng};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.StartOperation(authenticate, operationId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Start list of operation 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("startOperationList")]
        public async Task<object> StartOpearation([FromBody] List<StartOperationsDto> model)
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
            var serviceResult = await _operationService.StartOperation(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet, Route("pendingForConfimbadge")]
        public async Task<object> GetPendingForConfimOperationBadgeCountAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>();

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.GetPendingForConfirmOperationBadgeCountAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        [HttpGet, Route("inProgressbadge")]
        public async Task<object> GetInProgressOperationBadgeCountAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>
            {
                SCMRole.OperationInProgressGlbObs,
                SCMRole.OperationInProgressMng,
                SCMRole.OperationInProgressObs,
                SCMRole.OperationActivityUpd
            }; ;

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.GetInProgressOperationBadgeCountAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Add operation  attachment
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        [HttpPost, Route("Operation/{operationId:Guid}/attachment")]
        public async Task<object> AddOperationAttachmentAsync(Guid operationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.OperationInProgressMng,SCMRole.OperationActivityUpd};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var files = HttpContext.Request.Form.Files;
            var serviceResult = await _operationService.AddOperationAttachmentAsync(authenticate, operationId, files);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get operation attachment
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        [HttpGet, Route("Operation/{operationId:Guid}/PreparationAttachment")]
        public async Task<object> GetOperationAttachmentAsync(Guid operationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> {
                SCMRole.OperationInProgressGlbObs,
                SCMRole.OperationInProgressObs,
                SCMRole.OperationInProgressMng,
                SCMRole.OperationActivityUpd,
            };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.GetOperationAttachmentAsync(authenticate, operationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Delete operation attachment
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpDelete, Route("Operation/{operationId:Guid}/attachment")]
        public async Task<object> DeleteOperationAttachmentAsync(Guid operationId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.OperationInProgressMng};

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.DeleteOperationAttachmentAsync(authenticate, operationId, fileSrc);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download operation attachment
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="revisionId"></param>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Operation/{operationId:Guid}/downloadFile")]
        public async Task<object> DownloadOperationFileAsync(Guid operationId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string>
            {
                SCMRole.OperationInProgressGlbObs,
                SCMRole.OperationInProgressMng,
                SCMRole.OperationInProgressObs,
                SCMRole.OperationActivityUpd 
            };
            //var acceptType = new List<RevisionAttachmentType> { RevisionAttachmentType.Final, RevisionAttachmentType.FinalNative, RevisionAttachmentType.Preparation };

            //if (!acceptType.Contains(attachType))
            //    return BadRequest();
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _operationService.DownloadOperationFileAsync(authenticate, operationId, fileSrc);
            if (streamResult == null)
                return NotFound();

            //return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
            return File(streamResult.Stream, "application/octet-stream");
        }

        /// <summary>
        /// Confirm operation 
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        [HttpPut, Route("Confirm/{operationId:Guid}")]
        public async Task<object> ConfirmOperatinAsync(Guid operationId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.OperationInProgressMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.ConfirmOperationByOperationIdAsync(authenticate, operationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Abort operation 
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        [HttpPut, Route("Abort/{operationId:Guid}")]
        public async Task<object> AbortOperatinAsync(Guid operationId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.OperationInProgressMng };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.AbortOperationByOperationIdAsync(authenticate, operationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// re strart operation 
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        [HttpPut, Route("Restart/{operationId:Guid}")]
        public async Task<object> RestartOperatinAsync(Guid operationId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.OperationInProgressMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _operationService.RestartOperationByOperationIdAsync(authenticate, operationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
