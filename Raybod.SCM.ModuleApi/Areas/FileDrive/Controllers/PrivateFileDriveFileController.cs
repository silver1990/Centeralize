using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raybod.SCM.DataTransferObject.FileDriveDirectory;
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
    public class PrivateFileDriveFileController : ControllerBase
    {
        private readonly IFileDriveDirectoryService _fileDriveDirectoryService;
        private readonly IFileDriveFileService _fileDriveFileService;
        private readonly ILogger<PrivateFileDriveFileController> _logger;

        public PrivateFileDriveFileController(
            IFileDriveDirectoryService fileDriveDirectoryService,
            ILogger<PrivateFileDriveFileController> logger,IHttpContextAccessor httpContextAccessor, IFileDriveFileService fileDriveFileService)
        {
            _fileDriveDirectoryService = fileDriveDirectoryService;
            _logger = logger;
            _fileDriveFileService = fileDriveFileService;
           
               
           
        }
        /// <summary>
        /// Upload file to file drive
        /// </summary>
        /// <param name="directoryId"></param>
        /// <returns></returns>
        [Route("UploadDocument/{directoryId:Guid}")]
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<object> FileDriveUploadDocument(Guid directoryId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };
            var file = HttpContext.Request.Form.Files[0];
            var result = await _fileDriveFileService.FileDriveUploadFilePrivate(authenticate, directoryId, file);
            return result.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Download file from file drive
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [Route("downloadFile/{fileId:Guid}")]
        [HttpGet]
        public async Task<object> FileDriveDownloadDocumentAsync(Guid fileId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            var logInformation = LogerHelper.ActionExcuted(Request, authenticate, null);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };
            var streamResult = await _fileDriveFileService.FileDriveDownloadFilePrivate(authenticate, fileId);
            if (streamResult == null)
                return NotFound();

            return File(streamResult.Stream, streamResult.ContentType, streamResult.FileName);
        }

        /// <summary>
        /// Remove file drive file 
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpDelete, Route("removeFile/{fileId:Guid}")]
        public async Task<object> RemoveDirectoryAsync(Guid fileId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveFileService.DeleteFilePrivate(authenticate, fileId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

       
        /// <summary>
        /// Renaem file drive file
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("RenameFile/{fileId:Guid}")]
        [HttpPut]
        public async Task<object> UpdateFileAsync(Guid fileId, [FromBody] FileDriveFileRenameDto model)
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
            var serviceResult = await _fileDriveFileService.UpdateFilePrivate(authenticate, fileId, model);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }
        /// <summary>
        /// Move file drive file
        /// </summary>
        /// <param name="fileId"></param>
        ///<param name="destinationId"></param>
        /// <returns></returns>
        [HttpPost, Route("{fileId:Guid}/Movefile/{destinationId:Guid}")]
        public async Task<object> Movefile(Guid fileId, Guid destinationId)
        {

            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveFileService.MoveFileAsyncPrivate(authenticate, fileId, destinationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        /// <summary>
        /// Copy file drive file
        /// </summary>
        /// <param name="fileId"></param>
        ///<param name="destinationId"></param>
        /// <returns></returns>
        [HttpPost, Route("{fileId:Guid}/CopyFile/{destinationId:Guid}")]
        public async Task<object> CopyFile(Guid fileId, Guid destinationId)
        {
            var authenticate = HttpContextHelper.GetUserAuthenticateInfo(HttpContext);
            authenticate.Roles = new List<string> { SCMRole.PrivateMng };

            var logInformation = LogerHelper.ActionExcuted(Request, authenticate);
            _logger.LogWarning(logInformation.InformationText, logInformation.Args);
            var serviceResult = await _fileDriveFileService.CopyFileAsyncPrivate(authenticate, fileId, destinationId);
            return serviceResult.ToWebApiResultVCore(authenticate.language);
        }

        
    }
}
