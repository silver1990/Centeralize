using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Authentication;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Exon.TheWeb.Service.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Linq;

namespace Raybod.SCM.ModuleApi.Controllers
{
    [Route("api/raybodSCM/v1/account")]
    [ApiController]
    [SwaggerArea(AreaName = "raybodPanel")]
    public class SigninServiceController : ControllerBase
    {
        private readonly IAuthenticate _authenticate;
        private readonly ITeamWorkService _teamWorkService;
        private readonly IUserService _userService;
        private readonly ILogger<SigninServiceController> _logger;
        private readonly CompanyAppSettingsDto _appSettings;
        private readonly IFileDriveFileService _fileDriveFileService;
        private readonly IWebHostEnvironment _hostingEnvironmentRoot;

        public SigninServiceController(
            IAuthenticate authenticate,
            ILogger<SigninServiceController> logger, IHttpContextAccessor httpContextAccessor,
            ITeamWorkService teamWorkService,
            IOptions<CompanyAppSettingsDto> appSettings,
            IFileDriveFileService fileDriveFileService, IFileService fileService, IUserService userService, IWebHostEnvironment hostingEnvironmentRoot)
        {
            _authenticate = authenticate;
            _logger = logger;
            _teamWorkService = teamWorkService;
            _appSettings = appSettings.Value;
            _fileDriveFileService = fileDriveFileService;



            _userService = userService;
            _hostingEnvironmentRoot = hostingEnvironmentRoot;
        }

        /// <summary>
        /// sign in
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> Signin([FromBody]SigningApiDto model)
        {
            var authenticate = HttpContextHelper.GetLanguageWithoutAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
                    new ServiceResult<bool>(false, false,
                            new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                        .ToWebApiResultVCore(authenticate.language,ModelState);
            }

            var logInformation = LogerHelper.ActionSignInExcuted(Request, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var result = await _authenticate.IsAuthenticatedAsync(model, authenticate.language);
            return result;
        }

        /// <summary>
        /// get new refreshToken
        /// </summary>
        /// <param name="token"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getRefreshToken")]
        public async Task<object> RefreshToken(string token, string refreshToken)
        {
            var serviceResult = await _authenticate.RefreshTokenAsync(token, refreshToken);
            return serviceResult.ToHttpResponseV2();

        }

        
        [HttpGet]
        [Route("/Raybod/GetFileForIfram/{fileId:Guid}")]
        public async Task<object> GetFileForIfram(Guid fileId)
        {
   
            var streamResult = await _fileDriveFileService.FileDrivePreviewFile(fileId);
            if (streamResult!=null)
            {

                return new FileStreamResult(streamResult.Stream, streamResult.ContentType);
            }
                
            return NotFound();

        }


        [HttpGet]
        [Route("/Raybod/GetFileForPreview/{fileSrc}")]
        public async Task<object> GetFileForIfram(string fileSrc)
        {

            var streamResult = await _fileDriveFileService.GetPreviewFile(fileSrc);
            if (streamResult != null)
            {

                return new FileStreamResult(streamResult.Stream, streamResult.ContentType);
            }

            return NotFound();

        }
        [Authorize]
        [HttpGet, Route("CheckAuthentication")]
        public async Task<object> CheckAuthentication()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var result = await _userService.UserInfoInCheckAuthentication(authenticate);
            return result.ToWebApiResultVCore(authenticate.language);
            //return ServiceResultFactory.CreateSuccess(true);
        }
        
        [HttpGet, Route("GetCompanyName")]
        public async Task<object> GetCompanyName()
        {
            var authenticate = HttpContextHelper.GetLanguageWithoutAuthenticateInfo(HttpContext);
            var config = _appSettings.CompanyConfig.First(a => a.CompanyCode == authenticate.CompanyCode);
            return ServiceResultFactory.CreateSuccess(new { CompanyNameFA= config.CompanyNameFA, CompanyNameEN = config.CompanyNameEN });
        }
        [HttpPost]
        [Route("user/forgetPassword")]
        public async Task<object> UserForgetPasswordPassword([FromBody] ForgetPasswordModel model)
        {

            var authenticate = HttpContextHelper.GetLanguageWithoutAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _userService.ForgetPassword(model, authenticate.language);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        [HttpGet]
        [Route("writeAppSettings")]
        public async Task WriteAppSettings()
        {


            try
            {
                var filePath = Path.Combine(_hostingEnvironmentRoot.ContentRootPath, "appsettings.json");
                string json = System.IO.File.ReadAllText(filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                string connectionString = "{\"RonakDbContextConnection\": \"Data Source=.;Initial Catalog=PMIS-New;Integrated Security=false;user id=sa;password=Raybod123456;MultipleActiveResultSets=true\"}";
                var connectionStrings= Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj["ConnectionStrings"]);
                connectionStrings = connectionStrings.Substring(0, connectionStrings.Length-1);
                connectionStrings+=","+ connectionString.Substring(1);
                jsonObj["ConnectionStrings"] = Newtonsoft.Json.JsonConvert.DeserializeObject(connectionStrings);
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(filePath, output);
                Console.WriteLine(jsonObj);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing app settings | {0}", ex.Message);
            }

        }
        [HttpPost]
        [Route("user/validatePasswordRecoveryRequest")]
        public async Task<object> validatePasswordRecoveryRequest([FromBody] ValidateRecoveryPasswordDto model)
        {
            var authenticate = HttpContextHelper.GetLanguageWithoutAuthenticateInfo(HttpContext);

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _userService.ValidatePasswordRecoveryRequest(model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpPost]
        [Route("user/resetPassword")]
        public async Task<object> validatePasswordRecoveryRequest([FromBody] UserResetPasswordDto model)
        {
            var authenticate = HttpContextHelper.GetLanguageWithoutAuthenticateInfo(HttpContext);

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _userService.ResetPassworAsync(model);
           
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}