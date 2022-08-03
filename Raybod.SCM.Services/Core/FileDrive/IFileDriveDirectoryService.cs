using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.FileDriveDirectory;
using Raybod.SCM.DataTransferObject.FileDriveShare;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Utility.Utility.TreeModel;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
   public interface IFileDriveDirectoryService
    {
        #region Public

        Task<ServiceResult<FileDriveFileAndDirectoryListDto>> GetDirectoryInfoById(AuthenticateDto authenticate, Guid directoryId);
        Task<ServiceResult<FileDriveFileAndDirectoryListDto>> GetRootDirectoryInfo(AuthenticateDto authenticate);
        Task<ServiceResult<FileDriverDirectoryListDto>> CreateDirectory(AuthenticateDto authenticate, Guid? directoryId, FileDriveDirectoryCreateDto model);
        Task<ServiceResult<bool>> UpdateDirectory(AuthenticateDto authenticate, Guid directoryId, FileDriveDirectoryRenameDto model);
        Task<ServiceResult<bool>> DeleteDirectory(AuthenticateDto authenticate, Guid directoryId);
        Task<ServiceResult<bool>> DeleteDirectoryPermanently(AuthenticateDto authenticate, Guid directoryId);
        Task<ServiceResult<bool>> DeleteAllEntityPermanently(AuthenticateDto authenticate);
        Task<ServiceResult<List<ExpandoObject>>> GetDirectoryTreeAsync(AuthenticateDto authenticate);
        Task<ServiceResult<List<AdvanceSearchDto>>> GetAdvanceSearchDataAsync(AuthenticateDto authenticate);
        Task<ServiceResult<bool>> MoveDirectoryAsync(AuthenticateDto authenticate, Guid directoryId, Guid destinationId);
        Task<ServiceResult<bool>> RestoreDirectory(AuthenticateDto authenticate, Guid directoryId);
        Task<ServiceResult<bool>> CopyDirectoryAsync(AuthenticateDto authenticate, Guid directoryId, Guid destinationId);
        Task<ServiceResult<FileDriveTrashContentDto>> GetTrashContentAsync(AuthenticateDto authenticate);
        Task<DownloadFileDto> DownloadFolderAsync(AuthenticateDto authenticate, Guid directoryId);
        Task<ServiceResult<FileDriverDirectoryListDto>> FileDriveUploadFolder(AuthenticateDto authenticate, Guid directoryId, IFormFileCollection files);
        Task<ServiceResult<bool>> RestoreAllEntities(AuthenticateDto authenticate);

        #endregion


        #region Private
        Task<ServiceResult<FileDriveFileAndDirectoryListDto>> GetDirectoryInfoByIdPrivatly (AuthenticateDto authenticate, Guid directoryId);
        Task<ServiceResult<FileDriveFileAndDirectoryListDto>> GetRootDirectoryInfoPrivatly(AuthenticateDto authenticate);
        Task<ServiceResult<FileDriverDirectoryListDto>> CreateDirectoryPrivatly(AuthenticateDto authenticate, Guid? directoryId, FileDriveDirectoryCreateDto model);
        Task<ServiceResult<bool>> UpdateDirectoryPrivatly(AuthenticateDto authenticate, Guid directoryId, FileDriveDirectoryRenameDto model);
        Task<ServiceResult<bool>> DeleteDirectoryPrivatly(AuthenticateDto authenticate, Guid directoryId);
       
        Task<ServiceResult<List<ExpandoObject>>> GetDirectoryTreeAsyncPrivatly(AuthenticateDto authenticate);
        Task<ServiceResult<List<AdvanceSearchDto>>> GetAdvanceSearchDataAsyncPrivatly(AuthenticateDto authenticate);
        Task<ServiceResult<bool>> MoveDirectoryAsyncPrivatly(AuthenticateDto authenticate, Guid directoryId, Guid destinationId);
        Task<ServiceResult<bool>> CopyDirectoryAsyncPrivatly(AuthenticateDto authenticate, Guid directoryId, Guid destinationId);
        Task<DownloadFileDto> DownloadFolderAsyncPrivatly(AuthenticateDto authenticate, Guid directoryId);
        Task<ServiceResult<FileDriverDirectoryListDto>> FileDriveUploadFolderPrivatly(AuthenticateDto authenticate, Guid directoryId, IFormFileCollection files);
        Task<ServiceResult<bool>> AddSharePrivateAsync(AuthenticateDto authenticate, FileDriveShareCreateDto model);

        #endregion
    }
}
