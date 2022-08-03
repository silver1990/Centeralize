using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Consultant;
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

namespace Raybod.SCM.ModuleApi.Areas.SCMManagement.Controllers
{
    [Route("api/SCMManagement/v1/Consultant")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "Setting")]
    public class ConsultantController : ControllerBase
    {
        private readonly IConsultantService _consultantService;
        private readonly IUserService _userService;
        private readonly ILogger<ConsultantController> _logger;

        public ConsultantController(
            IConsultantService consultantService,
            ILogger<ConsultantController> logger,IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _consultantService = consultantService;
            _logger = logger;
            _userService = userService;
           
               
           
        }

        /// <summary>
        /// add consultant
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> AddConsultantAsync([FromBody] AddConsultantDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.CustomerMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _consultantService.AddConsultantAsync(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get consultant list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetConsultantAsync([FromQuery] ConsultantQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.CustomerMng, SCMRole.CustomerObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _consultantService.GetConsultantAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// edit consultant item
        /// </summary>
        /// <param name="consultantId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{consultantId:int}")]
        public async Task<object> EditConsultantAsync(int consultantId, [FromBody] AddConsultantDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            authenticate.Roles = new List<string> { SCMRole.CustomerMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _consultantService.EditConsultantAsync(authenticate, consultantId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        //        /// <summary>
        //        /// get conulting item
        //        /// </summary>
        //        /// <param name="consultantId"></param>
        //        /// <returns></returns>
        //        [HttpGet, Route("{consultantId:int}")]
        //        public async Task<object> GetConsultantByIdAsync(int consultantId)
        //        {
        //            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //            authenticate.Roles = new List<string> { SCMRole.CustomerMng, SCMRole.CustomerObs };

        //            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
        //            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

        //            var serviceResult = await _consultantService.GetConsultantByIdAsync(authenticate, consultantId);
        //            return serviceResult.ToWebApiResultVCore(authenticate.language);
        //        }

        /// <summary>
        /// delete consultant item
        /// </summary>
        /// <param name="consultantId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{consultantId:int}")]
        public async Task<object> DeleteCustomerAsync(int consultantId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.CustomerMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _consultantService.DeleteConsultantAsync(authenticate, consultantId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add consultant new user
        /// </summary>
        /// <param name="consultantId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{consultantId:int}/ConsultantUser")]
        public async Task<object> AddConsultantUserAsync(int consultantId, [FromBody] AddConsultantUserDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.CustomerMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _consultantService.AddConsultantUserAsync(authenticate, consultantId, model,true);

            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get consultant user
        /// </summary>
        /// <param name="consultantId"></param>
        /// <returns></returns>
        [HttpGet, Route("{consultantId:int}/ConsultantUser")]
        public async Task<object> GetConsultantUserAsync(int consultantId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.CustomerMng, SCMRole.CustomerObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _consultantService.GetConsultantUserAsync(authenticate, consultantId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// delete consultant user
        /// </summary>
        /// <param name="consultantId"></param>
        /// <param name="consultantUserId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{consultantId:int}/ConsultantUser/{consultantUserId:int}")]
        public async Task<object> DeleteCustomerUserByIdAsync(int consultantId, int consultantUserId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.CustomerMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _consultantService.DeleteConsultantUserByIdAsync(authenticate, consultantId, consultantUserId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
