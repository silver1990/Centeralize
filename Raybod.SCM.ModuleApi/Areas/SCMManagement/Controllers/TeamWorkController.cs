using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.Authentication;
using Raybod.SCM.DataTransferObject.Contract;
using Raybod.SCM.DataTransferObject.TeamWork;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.ModuleApi.Helper;
using Raybod.SCM.ModuleApi.Helper.Authentication;
using Raybod.SCM.ModuleApi.Helper.Logger;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Filters;
using Microsoft.AspNetCore.Http;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.DataTransferObject.Notification;

namespace Raybod.SCM.ModuleApi.Areas.SCMManagement.Controllers
{
    [Route("api/SCMManagement/v1/teamWork")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "Setting")]
    public class TeamWorkController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IProductGroupService _productGroupService;
        private readonly IContractDocumentGroupService _documentGroupService;
        private readonly IContractOperationGroupService _operationGroupService;
        private readonly ITeamWorkService _teamWorkService;
        private readonly IContractService _contractService;
        private IUserNotifyService _notifyService;
        private readonly ILogger<TeamWorkController> _logger;
        private string language="";
        public TeamWorkController(IUserService userService,
            IProductGroupService productGroupService,
            ITeamWorkService teamWorkService,
            IContractService contractService,
            IContractDocumentGroupService documentGroupService,
            ILogger<TeamWorkController> logger, IHttpContextAccessor httpContextAccessor, IContractOperationGroupService operationGroupService, IUserNotifyService notifyService)
        {
            _userService = userService;
            _productGroupService = productGroupService;
            _documentGroupService = documentGroupService;
            _teamWorkService = teamWorkService;
            _contractService = contractService;
            _logger = logger;
            _operationGroupService = operationGroupService;
            _notifyService = notifyService;
        }

        /// <summary>
        /// get user list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("users")]
        public async Task<object> GetUserAsync([FromQuery] UserQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng, SCMRole.TeamworkObs };
            
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _userService.GetUserAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get productGroup List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("getProductGroups")]
        public async Task<object> GetProductGroups(string query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng, SCMRole.TeamworkObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _productGroupService.GetProductGroupListWithoutLimitedBypermissionProductGroupAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get documentGroupList
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("getDocumentGroups")]
        public async Task<object> GetDocumentGroupsAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng, SCMRole.TeamworkObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _documentGroupService.GetDocumentGroupListWithoutLimitedBypermissionDocumentGroupAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get operation group list
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("getOperationGroups")]
        public async Task<object> getOperationGroups()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng, SCMRole.TeamworkObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _operationGroupService.GetOperationGroupListWithoutLimitedBypermissionOperationGroupAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get contract List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("GetContracts")]
        public async Task<object> GetContractForCraeteNewTeamWorkAsync([FromQuery] ContractQuery query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng, SCMRole.TeamworkObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _contractService.GetContractForCraeteNewTeamWorkAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add new teamWork
        /// </summary>
        /// <param name="contractCode"></param>
        /// <returns></returns>
        [HttpPost, Route("{contractCode}")]
        [Permission(Role = SCMRole.TeamworkMng)]
        public async Task<object> AddTeamWorkAsync(string contractCode)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, contractCode);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _teamWorkService.AddTeamWorkAsync(authenticate, contractCode);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get teamWork List
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetAllTeamWorkAsync([FromQuery] TeamWorkQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng, SCMRole.TeamworkObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _teamWorkService
                .GetAllTeamWorkAsync(authenticate, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// add new user to teamWork
        /// </summary>
        /// <param name="teamWorkId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        [HttpPost, Route("{teamWorkId:int}/addUser")]
        public async Task<object> AddUserToTeamWorkAsync(int teamWorkId, [FromBody] List<int> userIds)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, userIds);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _teamWorkService.AddUserToTeamWorkAsync(authenticate, teamWorkId, userIds);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// set user ProductGroup Limited
        /// </summary>
        /// <param name="teamWorkId"></param>
        /// <param name="userId"></param>
        /// <param name="productGroupIds"></param>
        /// <returns></returns>
        [HttpPost, Route("{teamWorkId:int}/setProductGroup/{userId:int}")]
        public async Task<object> SetTeamWorkUserProductGroupAsync(int teamWorkId, int userId, [FromBody] List<int> productGroupIds)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, productGroupIds);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _teamWorkService.SetTeamWorkUserProductGroupAsync(authenticate, teamWorkId, userId, productGroupIds);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// set user DocumentGroup Limited
        /// </summary>
        /// <param name="teamWorkId"></param>
        /// <param name="userId"></param>
        /// <param name="documentGroupIds"></param>
        /// <returns></returns>
        [HttpPost, Route("{teamWorkId:int}/setDocumentGroup/{userId:int}")]
        public async Task<object> SetTeamWorkUserDocumentGroupAsync(int teamWorkId, int userId, [FromBody] List<int> documentGroupIds)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, documentGroupIds);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _teamWorkService.SetTeamWorkUserDocumentGroupAsync(authenticate, teamWorkId, userId, documentGroupIds);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// set user DocumentGroup Limited
        /// </summary>
        /// <param name="teamWorkId"></param>
        /// <param name="userId"></param>
        /// <param name="operationGroupIds"></param>
        /// <returns></returns>
        [HttpPost, Route("{teamWorkId:int}/setOperationGroup/{userId:int}")]
        public async Task<object> SetTeamWorkUserOperationGroupAsync(int teamWorkId, int userId, [FromBody] List<int> operationGroupIds)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, operationGroupIds);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _teamWorkService.SetTeamWorkUserOperationGroupAsync(authenticate, teamWorkId, userId, operationGroupIds);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// set user teamWork role
        /// </summary>
        /// <param name="teamWorkId"></param>
        /// <param name="userId"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        [HttpPost, Route("{teamWorkId:int}/setRole/{userId:int}")]
        public async Task<object> SetUserTeamWorkRoleAsync(int teamWorkId, int userId, [FromBody] List<BaseUserRoleDto> roles)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, roles);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _teamWorkService.SetUserTeamWorkRoleAsync(authenticate, teamWorkId, userId, roles);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// get teamWork users list with details
        /// </summary>
        /// <param name="teamWorkId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Route("{teamWorkId:int}")]
        public async Task<object> GetTeamWorkUserListByTeamWorkIdAsync(int teamWorkId, [FromQuery] TeamWorkQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> ();

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _teamWorkService.GetTeamWorkUserListByTeamWorkIdAsync(authenticate, teamWorkId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Delete TeamWork User 
        /// </summary>
        /// <param name="teamWorkId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{teamWorkId:int}/User/{userId:int}")]
        public async Task<object> DeleteTeamWorkUserPermissions(int teamWorkId, int userId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult =
                await _teamWorkService.DeleteUserFromTeamWorkAsync(authenticate, teamWorkId, userId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Set TeamWork User Config (show/hide)
        /// </summary>
        /// <param name="teamWorks"></param>
        /// <returns></returns>
        //[HttpPost]
        //[Route("setTeamWorkConfig")]
        //public async Task<object> SetTeamWorkUserConfigAsync([FromBody] List<SetTeamWorkUserConfigDto> teamWorks)
        //{
        //    var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
        //    if (!ModelState.IsValid)
        //    {
        //        return
        //            new ServiceResult<bool>(false, false,
        //                    new List<ServiceMessage> { new ServiceMessage(type: MessageType.Error, MessageId.ModelStateInvalid) })
        //                .ToWebApiResultVCore(authenticate.language,ModelState);
        //    }

        //    //authenticate.Roles = new List<string> { SCMRole.TeamworkManagement };

        //    var logInformation = LogerHelper.ActionExcuted(Request, authenticate, teamWorks);
        //    _logger.LogWarning(logInformation.InformationText, logInformation.Args);

        //    var serviceResult = await _teamWorkService.SetTeamWorkUserConfigAsync(authenticate, teamWorks);
        //    return serviceResult.ToWebApiResultVCore(authenticate.language);
        //}

        /// <summary>
        /// get teamWork list by user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("userTeamWork")]
        public async Task<object> GetUserTeamWorkAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng, SCMRole.TeamworkObs };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _teamWorkService.GetUserTeamWorkAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet, Route("{teamworkId:int}/userNotify/{userId:int}")]
        public async Task<object> GetUserEmailNotifiesAsync(int teamworkId, int userId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng };
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _notifyService.GetUserNotifies(authenticate,  teamworkId,userId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        [HttpPost, Route("{teamworkId:int}/userNotify/{userId:int}")]
        public async Task<object> UpdateUserEmailNotifiesAsync([FromBody] UserNotifyListDto model, int teamworkId, int userId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            authenticate.Roles = new List<string> { SCMRole.TeamworkMng };
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _notifyService.UpdateUserNotifies(authenticate, model, teamworkId,userId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}