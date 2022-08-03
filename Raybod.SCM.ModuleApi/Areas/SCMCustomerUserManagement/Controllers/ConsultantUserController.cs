using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Consultant;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Raybod.SCM.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.ModuleApi.Areas.SCMCustomerUserManagement.Controllers
{
    [Route("api/SCMCustomerUserManagement/v1/ConsultantUser")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "SCMCustomerPanelManagement")]
    public class ConsultantUserController : ControllerBase
    {

        private readonly IConsultantService _consultantService;
        private readonly IUserService _userService;
        private readonly ILogger<ConsultantUserController> _logger;
        private readonly ITeamWorkService _teamWorkService;

        public ConsultantUserController(ILogger<ConsultantUserController> logger, IHttpContextAccessor httpContextAccessor, IUserService userService, ITeamWorkService teamWorkService, IConsultantService consultantService)
        {
            _logger = logger;
            _userService = userService;
            _teamWorkService = teamWorkService;
            _consultantService = consultantService;



        }

        /// <summary>
        /// add consultant new user
        /// </summary>
        /// <param name="consultantId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("{consultantId:int}/ConsultantUser/Create")]
        public async Task<object> AddConsultantUserAsync(int consultantId, [FromBody] AddConsultantUserDto model)
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
                configuration.CreateMap<AddConsultantUserDto, AddUserDto>()

                .ForMember(destination => destination.UserName, option => option.MapFrom(source => source.Email));
            });

            var mapper = mapperConfiguration.CreateMapper();
            var userModel = mapper.Map<AddUserDto>(model);
            userModel.Password = GeneratePassword.RandomString(6);
            userModel.Mobile = "09000000000";

            authenticate.Roles = new List<string> { SCMRole.CustomerMng, SCMRole.UserMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _consultantService.AddConsultantUserAsync(authenticate, consultantId, model, true, userModel, UserStatus.ConsultantUser);

            return serviceResult.ToWebApiResultVCore(authenticate.language);



        }

        /// <summary>
        /// edit consultant user
        /// </summary>
        /// <param name="consultantId"></param>
        /// <param name="companyUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("{consultantId:int}/ConsultantUser/{companyUserId:int}")]
        public async Task<object> EditConsultantUserAsync(int consultantId, int companyUserId, [FromBody] EditConsultantUserDto model)
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
                configuration.CreateMap<EditConsultantUserDto, AddUserDto>()

                .ForMember(destination => destination.UserName, option => option.MapFrom(source => source.Email));
            });

            var mapper = mapperConfiguration.CreateMapper();
            var userModel = mapper.Map<AddUserDto>(model);
            userModel.Password = GeneratePassword.RandomString(6);
            userModel.Mobile = "09000000000";


                authenticate.Roles = new List<string> { SCMRole.CustomerMng, SCMRole.UserMng };

                var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
                _logger.LogWarning(logInformation.InformationText, logInformation.Args);

                var serviceResult = await _consultantService.EditConsultantUserAsync(authenticate, consultantId, model, companyUserId,userModel,UserStatus.ConsultantUser,model.IsCustomerUser);

                return serviceResult.ToWebApiResultVCore(authenticate.language);
           


        }

        /// <summary>
        /// delete consultant user
        /// </summary>
        /// <param name="consultantId"></param>
        /// <param name="consultantUserId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{consultantId:int}/ConsultantUser/{consultantUserId:int}")]
        public async Task<object> DeleteConsultantUserByIdAsync(int consultantId, int consultantUserId)
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
