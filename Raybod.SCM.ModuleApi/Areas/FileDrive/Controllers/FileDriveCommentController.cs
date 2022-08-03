using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.FileDriveDirectory;
using Raybod.SCM.DataTransferObject.FileDriveShare;
using Raybod.SCM.Domain.Enum;
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

namespace Raybod.SCM.ModuleApi.Areas.FileDrive.Controllers
{
    [Route("api/fileDrive/v1")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "fileDrive")]
    public class FileDriveCommentController : ControllerBase
    {
        private readonly IFileDriveCommentService _filedriveCommentService;
        private readonly IUserService _userService;

        private readonly ILogger<FileDriveCommentController> _logger;

        public FileDriveCommentController(
            IFileDriveCommentService filedriveCommentService,
            IUserService userService,
            ILogger<FileDriveCommentController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _filedriveCommentService = filedriveCommentService;
            _userService = userService;
            _logger = logger;

        }
        [Route("{fileId:Guid}/fileDriveComment")]
        [HttpGet]
        public async Task<object> GetFileDriveComment(Guid fileId, [FromQuery] FileDriveCommentQueryDto query)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, query);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _filedriveCommentService.GetFileDriveCommentAsync(authenticate, fileId, query);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        [Route("{fileId:Guid}/fileDriveComment")]
        [HttpPost]
        public async Task<object> AddFileDriveCommentAsync(Guid fileId, [FromBody] AddFileDriveCommentDto model)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language, ModelState);
            }

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _filedriveCommentService.AddFileDriveCommentAsync(authenticate, fileId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        [HttpGet]
        [Route("fileDriveComment/{commentId:long}/downloadFile")]
        public async Task<object> DownloadCommentFileAsync(long commentId, string fileSrc)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, fileSrc);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _filedriveCommentService.DownloadCommentFileAsync(authenticate, commentId, fileSrc);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }


        [HttpGet]
        [Route("{fileId:Guid}/fileDriveComment/userMention")]
        public async Task<object> GetUserMentionOfFileDriveCommentAsync(Guid fileId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            authenticate.Roles = new List<string> { SCMRole.FileDriveMng, SCMRole.FileDriveMng };
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _filedriveCommentService.GetUserMentionsAsync(authenticate,fileId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
