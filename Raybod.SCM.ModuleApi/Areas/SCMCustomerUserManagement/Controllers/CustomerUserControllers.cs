using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject._admin.Customer;
using Raybod.SCM.DataTransferObject.Consultant;
using Raybod.SCM.DataTransferObject.Customer;
using Raybod.SCM.DataTransferObject.TeamWork;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.Utility.Filters;
using Raybod.SCM.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Raybod.SCM.ModuleApi.Areas.SCMManagement.Controllers
{
    [Route("api/SCMCustomerUserManagement/v1/CustomerUser")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "SCMCustomerPanelManagement")]
    public class CustomerUserControllers : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IConsultantService _consultantService;
        private readonly IUserService _userService;
        private readonly ILogger<CustomerController> _logger;
        private readonly ITeamWorkService _teamWorkService;

        public CustomerUserControllers(
            ICustomerService customerService,
            ILogger<CustomerController> logger, IHttpContextAccessor httpContextAccessor, IUserService userService, ITeamWorkService teamWorkService, IConsultantService consultantService)
        {
            _customerService = customerService;
            _logger = logger;
            _userService = userService;
            _teamWorkService = teamWorkService;
            _consultantService = consultantService;



        }
        [HttpGet, Route("GetCustomerUsers/{contractCode}")]
        public async Task<object> GetUsers(string contractCode, [FromQuery] UserQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.UserObs, SCMRole.UserMng };
            authenticate.ContractCode = contractCode;
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _userService.GetUserForCustomerUsersAsync(authenticate, query, new List<int> { (int)UserStatus.CustomerUser, (int)UserStatus.ConsultantUser });
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add customer new user
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{customerId:int}/CustomerUser/Create")]
        public async Task<object> AddCustomerUserAsync(int customerId, [FromBody] AddCustomerUserDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language, ModelState);
            }

            var mapperConfiguration = new MapperConfiguration(configuration =>
            {
                configuration.CreateMap<AddCustomerUserDto, AddUserDto>()

                .ForMember(destination => destination.UserName, option => option.MapFrom(source => source.Email));
            });

            var mapper = mapperConfiguration.CreateMapper();
            var userModel = mapper.Map<AddUserDto>(model);
            userModel.Password = GeneratePassword.RandomString(6);
            userModel.Mobile = "09000000000";

            authenticate.Roles = new List<string> { SCMRole.CustomerMng, SCMRole.UserMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _customerService.AddCustomerUserAsync(authenticate, customerId, model, true, userModel, UserStatus.CustomerUser);

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


        /// <summary>
        /// edit customer new user
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="companyUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{customerId:int}/CustomerUser/{companyUserId:int}")]
        public async Task<object> EditCustomerUserAsync(int customerId, int companyUserId, [FromBody] EditCustomerUserDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language, ModelState);
            }


            var mapperConfiguration = new MapperConfiguration(configuration =>
            {
                configuration.CreateMap<EditCustomerUserDto, AddUserDto>()

                .ForMember(destination => destination.UserName, option => option.MapFrom(source => source.Email));
            });

            var mapper = mapperConfiguration.CreateMapper();
            var userModel = mapper.Map<AddUserDto>(model);
            userModel.Password = GeneratePassword.RandomString(6);
            userModel.Mobile = "09000000000";
            authenticate.Roles = new List<string> { SCMRole.CustomerMng, SCMRole.UserMng };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _customerService.EditCustomerUserAsync(authenticate, customerId, model, companyUserId, userModel, UserStatus.CustomerUser, model.IsCustomerUser);

            return serviceResult.ToWebApiResultVCore(authenticate.language);



        }

    }
}
