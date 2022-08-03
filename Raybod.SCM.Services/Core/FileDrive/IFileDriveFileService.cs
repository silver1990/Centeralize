using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.FileDriveDirectory;
using Raybod.SCM.Services.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IFileDriveFileService
    {
        #region Public
        Task<ServiceResult<FileDriveFilesListDto>> FileDriveUploadFile(AuthenticateDto authenticate, Guid directoryId, IFormFile file);
        Task<DownloadFileDto> FileDriveDownloadFile(AuthenticateDto authenticate, Guid fileId);
        Task<DownloadFileDto> FileDrivePreviewFile(Guid fileId);
        Task<DownloadFileDto> GetPreviewFile(string fileSrc);
        Task<ServiceResult<bool>> DeleteFile(AuthenticateDto authenticate, Guid fileId);
        Task<ServiceResult<bool>> DeleteFilePermanently(AuthenticateDto authenticate, Guid fileId);
        Task<ServiceResult<bool>> RestoreFile(AuthenticateDto authenticate, Guid fileId);
        Task<ServiceResult<bool>> UpdateFile(AuthenticateDto authenticate, Guid fileId, FileDriveFileRenameDto model);
        Task<ServiceResult<bool>> MoveFileAsync(AuthenticateDto authenticate, Guid fileId, Guid destinationId);
        Task<ServiceResult<bool>> CopyFileAsync(AuthenticateDto authenticate, Guid fileId, Guid destinationId);

        #endregion


        #region Private
        Task<ServiceResult<FileDriveFilesListDto>> FileDriveUploadFilePrivate(AuthenticateDto authenticate, Guid directoryId, IFormFile file);
        Task<DownloadFileDto> FileDriveDownloadFilePrivate(AuthenticateDto authenticate, Guid fileId);
        Task<DownloadFileDto> FileDrivePreviewFilePrivate(Guid fileId);
        Task<ServiceResult<bool>> DeleteFilePrivate(AuthenticateDto authenticate, Guid fileId);

        Task<ServiceResult<bool>> UpdateFilePrivate(AuthenticateDto authenticate, Guid fileId, FileDriveFileRenameDto model);
        Task<ServiceResult<bool>> MoveFileAsyncPrivate(AuthenticateDto authenticate, Guid fileId, Guid destinationId);
        Task<ServiceResult<bool>> CopyFileAsyncPrivate(AuthenticateDto authenticate, Guid fileId, Guid destinationId);
        #endregion
    }
}
