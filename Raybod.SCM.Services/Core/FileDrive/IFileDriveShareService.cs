using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.FileDriveDirectory;
using Raybod.SCM.DataTransferObject.FileDriveShare;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IFileDriveShareService
    {
        Task<ServiceResult<bool>> AddShareAsync(AuthenticateDto authenticate,FileDriveShareCreateDto model);
        Task<ServiceResult<FileDriveFileAndDirectoryListDto>> GetShareEntitiesAsync(AuthenticateDto authenticate);
        Task<ServiceResult<FileDriveSharedUserListDto>> GetShareForEntityByEntityIdAsync(AuthenticateDto authenticate,Guid entityId,EntityType entity);
        Task<ServiceResult<FileDriverDirectoryListDto>> CreateShareDirectoryAsync(AuthenticateDto authenticate, Guid? directoryId, FileDriveDirectoryCreateDto model);
        Task<DownloadFileDto> FileDriveShareDownloadFile(AuthenticateDto authenticate, Guid fileId);
        Task<ServiceResult<bool>> UpdateShareFileAsync(AuthenticateDto authenticate, Guid fileId, FileDriveFileRenameDto model);
        Task<ServiceResult<bool>> UpdateShareDirectoryAsync(AuthenticateDto authenticate, Guid directoryId, FileDriveDirectoryRenameDto model);
        Task<DownloadFileDto> DownloadShareFolderAsync(AuthenticateDto authenticate, Guid directoryId);
        Task<ServiceResult<FileDriveFileAndDirectoryListDto>> GetShareDirectoryInfoByIdAsync(AuthenticateDto authenticate, Guid directoryId);
        Task<ServiceResult<FileDriverDirectoryListDto>> FileDriveUploadShareFolderAsync(AuthenticateDto authenticate, Guid directoryId, IFormFileCollection files);
        Task<ServiceResult<FileDriveFilesListDto>> FileDriveUploadShareFileAsync(AuthenticateDto authenticate, Guid directoryId, IFormFile file);
        Task<ServiceResult<bool>> DeleteShareDirectoryAsync(AuthenticateDto authenticate, Guid directoryId);
        Task<ServiceResult<bool>> DeleteShareFileAsync(AuthenticateDto authenticate, Guid fileId);
    }
}
