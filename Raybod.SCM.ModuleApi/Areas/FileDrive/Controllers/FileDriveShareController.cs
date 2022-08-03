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
    public class FileDriveShareController : ControllerBase
    {
        private readonly IFileDriveShareService _fileDriveShareService;
        private readonly ITeamWorkService _teamWorkService;
        private readonly ILogger<FileDriveShareController> _logger;

        public FileDriveShareController(
            IFileDriveShareService fileDriveShareService,
            ILogger<FileDriveShareController> logger,IHttpContextAccessor httpContextAccessor, ITeamWorkService teamWorkService)
        {
            _fileDriveShareService = fileDriveShareService;
            _logger = logger;
            _teamWorkService = teamWorkService;
           
               
           
        }

        /// <summary>
        /// Get file share
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("getShareEntities")]
        public async Task<object> GetFileShareInfoAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            authenticate.Roles = new List<string> ();
            var serviceResult = await _fileDriveShareService.GetShareEntitiesAsync(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get share directory
        /// </summary>
        /// <param name="directoryId"></param>
        /// <returns></returns>
        [HttpGet, Route("getShareDirectoryInfo/{directoryId:Guid}")]
        public async Task<object> GetShareDirectoryInfoByDirectoryIdAsync(Guid directoryId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };
            var serviceResult = await _fileDriveShareService.GetShareDirectoryInfoByIdAsync(authenticate, directoryId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Create file share
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("createShare")]
        public async Task<object> CrateShareAsync([FromBody] FileDriveShareCreateDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveShareService.AddShareAsync(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Get shared users
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        [HttpGet, Route("GetSharedUsers/{entityId:Guid}/{entityType:int}")]
        public async Task<object> GetFileShareInfoAsync(Guid entityId,int entityType)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            authenticate.Roles = new List<string>();
            var serviceResult = await _fileDriveShareService.GetShareForEntityByEntityIdAsync(authenticate,entityId,(EntityType)entityType);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// get teamWork list for file share
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("FileDriveShareTeamWork/{entityId:Guid}/{entityType:int}")]
        public async Task<object> GetUserTeamWorkForFileDriveShareAsync(Guid entityId, int entityType)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var serviceResult = await _teamWorkService.GetUserTeamWorkForFileShareAsync(authenticate,entityId,(EntityType)entityType);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Create share directory
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("createShareDirectory")]
        public async Task<object> CrateShareDirectoryAsync([FromBody] FileDriveDirectoryCreateDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveShareService.CreateShareDirectoryAsync(authenticate, model.DirectoryId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download share file
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [Route("downloadShareFile/{fileId:Guid}")]
        [HttpGet]
        public async Task<object> FileDriveShareDownloadDocumentAsync(Guid fileId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };
            var streamResult = await _fileDriveShareService.FileDriveShareDownloadFile(authenticate, fileId);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }

        /// <summary>
        /// Renaem share file
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("RenameShareFile/{fileId:Guid}")]
        [HttpPut]
        public async Task<object> UpdateShareFileAsync(Guid fileId, [FromBody] FileDriveFileRenameDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveShareService.UpdateShareFileAsync(authenticate, fileId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Rename share directory 
        /// </summary>
        /// <param name="directoryId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("renameShareDirectory/{directoryId:Guid}")]
        public async Task<object> RenameDirectoryAsync(Guid directoryId, [FromBody] FileDriveDirectoryRenameDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            if (!ModelState.IsValid)
            {
                return
               new ServiceResult<bool>(false, false,
                       new List<ServiceMessage> { new ServiceMessage(MessageType.Error, MessageId.ModelStateInvalid) })
                   .ToWebApiResultVCore(authenticate.language,ModelState);
            }


            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, model);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveShareService.UpdateShareDirectoryAsync(authenticate, directoryId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download share directory
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("DownloadShareDirectory/{directoryId:Guid}")]
        public async Task<object> DownloadDirectory(Guid directoryId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _fileDriveShareService.DownloadShareFolderAsync(authenticate, directoryId);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }
        /// <summary>
        /// Upload folder to file drive
        /// </summary>
        /// <param name="directoryId"></param>
        /// <returns></returns>
        [Route("UploadShareFolder/{directoryId:Guid}")]
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<object> FileDriveShareFolderUpload(Guid directoryId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };
            var files = HttpContext.Request.Form.Files;
            var result = await _fileDriveShareService.FileDriveUploadShareFolderAsync(authenticate, directoryId, files);
            return result.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Upload share file
        /// </summary>
        /// <param name="directoryId"></param>
        /// <returns></returns>
        [Route("UploadShareDocument/{directoryId:Guid}")]
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<object> FileDriveUploadDocument(Guid directoryId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);

            var file = HttpContext.Request.Form.Files[0];
            var result = await _fileDriveShareService.FileDriveUploadShareFileAsync(authenticate, directoryId, file);
            return result.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Remove share directory 
        /// </summary>
        /// <param name="directoryId"></param>
        /// <returns></returns>
        [HttpDelete, Route("share/removeShareDirectory/{directoryId:Guid}")]
        public async Task<object> RemoveShareDirectoryAsync(Guid directoryId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveShareService.DeleteShareDirectoryAsync(authenticate, directoryId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Remove share file 
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpDelete, Route("share/removeShareFile/{fileId:Guid}")]
        public async Task<object> RemoveDirectoryAsync(Guid fileId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveShareService.DeleteShareFileAsync(authenticate, fileId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
