using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Customer;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.SCMManagement.Controllers
{
    [Route("api/SCMManagement/v1/Customer")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "Setting")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IUserService _userService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            ICustomerService customerService,
            ILogger<CustomerController> logger,IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _customerService = customerService;
            _logger = logger;
            _userService = userService;
           
               
           
        }

        /// <summary>
        /// add customer
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> AddCustomerAsync([FromBody] AddCustomerDto model)
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

            var serviceResult = await _customerService.AddCustomerAsync(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get customer list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetCustomerAsync([FromQuery] CustomerQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.CustomerMng, SCMRole.CustomerObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _customerService.GetCustomerAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// edit customer item
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{customerId:int}")]
        public async Task<object> EditCustomerAsync(int customerId, [FromBody] AddCustomerDto model)
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

            var serviceResult = await _customerService.EditCustomerAsync(authenticate, customerId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get customer item
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet, Route("{customerId:int}")]
        public async Task<object> GetCustomerByIdAsync(int customerId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.CustomerMng, SCMRole.CustomerObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _customerService.GetCustomerByIdAsync(authenticate, customerId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// delete customer item
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{customerId:int}")]
        public async Task<object> DeleteCustomerAsync(int customerId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.CustomerMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _customerService.DeleteCustomerAsync(authenticate, customerId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add customer new user
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{customerId:int}/CustomerUser")]
        public async Task<object> AddCustomerUserAsync(int customerId, [FromBody] AddCustomerUserDto model)
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

            var serviceResult = await _customerService.AddCustomerUserAsync(authenticate, customerId, model,true);

            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get customer user
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet, Route("{customerId:int}/CustomerUser")]
        public async Task<object> GetCustomerUserAsync(int customerId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.CustomerMng, SCMRole.CustomerObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _customerService.GetCustomerUserAsync(authenticate, customerId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// delete customer user
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="customerUserId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{customerId:int}/CustomerUser/{customerUserId:int}")]
        public async Task<object> DeleteCustomerUserByIdAsync(int customerId, int customerUserId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.CustomerMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _customerService.DeleteCustomerUserByIdAsync(authenticate, customerId, customerUserId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}