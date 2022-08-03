using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.FileDriveDirectory;
using Raybod.SCM.DataTransferObject.FileDriveShare;
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
    [Route("api/fileDrive/v1/private")]
    [Authorize]
    [Raybod.SCM.Utility.Security.Authorize(0)]
    [ApiController]
    [SwaggerArea(AreaName = "fileDrive")]
    public class PrivateFileDriveDirectoryController : ControllerBase
    {
        private readonly IFileDriveDirectoryService _fileDriveDirectoryService;
        private readonly ILogger<PrivateFileDriveDirectoryController> _logger;

        public PrivateFileDriveDirectoryController(
            IFileDriveDirectoryService fileDriveDirectoryService,
            ILogger<PrivateFileDriveDirectoryController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _fileDriveDirectoryService = fileDriveDirectoryService;
            _logger = logger;
           
               
           
        }


        /// <summary>
        /// Get file drive root directory
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("getFileDriveRoot")]
        public async Task<object> GetFileDriveRootInfoAsync()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng};
            var serviceResult = await _fileDriveDirectoryService.GetRootDirectoryInfoPrivatly(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get file drive root directory
        /// </summary>
        /// <param name="directoryId"></param>
        /// <returns></returns>
        [HttpGet, Route("getFileDriveDirectoryInfo/{directoryId:Guid}")]
        public async Task<object> GetFileDriveInfoByDirectoryIdAsync(Guid directoryId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };
            var serviceResult = await _fileDriveDirectoryService.GetDirectoryInfoByIdPrivatly(authenticate, directoryId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Create file drive directory 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("createDirectory")]
        public async Task<object> CrateDirectoryAsync([FromBody] FileDriveDirectoryCreateDto model)
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
            var serviceResult = await _fileDriveDirectoryService.CreateDirectoryPrivatly(authenticate, model.DirectoryId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Rename file drive directory 
        /// </summary>
        /// <param name="directoryId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut, Route("renameDirectory/{directoryId:Guid}")]
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
            var serviceResult = await _fileDriveDirectoryService.UpdateDirectoryPrivatly(authenticate, directoryId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Remove file drive directory 
        /// </summary>
        /// <param name="directoryId"></param>
        /// <returns></returns>
        [HttpDelete, Route("removeDirectory/{directoryId:Guid}")]
        public async Task<object> RemoveDirectoryAsync(Guid directoryId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveDirectoryService.DeleteDirectoryPrivatly(authenticate, directoryId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
       
        /// <summary>
        /// Get file drive directory Tree 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetDirectoryTree")]
        public async Task<object> GetDirectoryTree()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveDirectoryService.GetDirectoryTreeAsyncPrivatly(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get file drive data for advance search 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetAdvanceSearchData")]
        public async Task<object> GetAdvanceSearchData()
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveDirectoryService.GetAdvanceSearchDataAsyncPrivatly(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Get to directory from advance search 
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetToDirctoryFromSearch")]
        public async Task<object> GetToDirctoryFromSearch([FromBody] AdvanceSearchDto model)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveDirectoryService.GetAdvanceSearchDataAsyncPrivatly(authenticate);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Move file drive directory
        /// </summary>
        /// <param name="directoryId"></param>
        ///<param name="destinationId"></param>
        /// <returns></returns>
        [HttpPost, Route("{directoryId:Guid}/MoveDirectory/{destinationId:Guid}")]
        public async Task<object> MoveDirectory(Guid directoryId, Guid destinationId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveDirectoryService.MoveDirectoryAsyncPrivatly(authenticate, directoryId, destinationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Copy file drive directory
        /// </summary>
        /// <param name="directoryId"></param>
        ///<param name="destinationId"></param>
        /// <returns></returns>
        [HttpPost, Route("{directoryId:Guid}/CopyDirectory/{destinationId:Guid}")]
        public async Task<object> CopyDirectory(Guid directoryId, Guid destinationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveDirectoryService.CopyDirectoryAsyncPrivatly(authenticate, directoryId, destinationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }


        /// <summary>
        /// Get file drive trash Content 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("DownloadDirectory/{directoryId}")]
        public async Task<object> DownloadDirectory(Guid directoryId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var streamResult = await _fileDriveDirectoryService.DownloadFolderAsyncPrivatly(authenticate, directoryId);
            if (streamResult == null)
                return NotFound();
            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }

        /// <summary>
        /// Upload folder to file drive
        /// </summary>
        /// <param name="directoryId"></param>
        /// <returns></returns>
        [Route("UploadFolder/{directoryId:Guid}")]
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<object> FileDriveUploadFolder(Guid directoryId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };
            var files = HttpContext.Request.Form.Files;
            var result = await _fileDriveDirectoryService.FileDriveUploadFolderPrivatly(authenticate, directoryId, files);
            return result.ToWebApiResultVCore(authenticate.language);
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
            var serviceResult = await _fileDriveDirectoryService.AddSharePrivateAsync(authenticate, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
    }
}
