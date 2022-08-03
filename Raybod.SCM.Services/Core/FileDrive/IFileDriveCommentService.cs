using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.FileDriveDirectory;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Services.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IFileDriveCommentService
    {
        Task<ServiceResult<string>> AddFileDriveCommentAsync(AuthenticateDto authenticate, Guid fileId,AddFileDriveCommentDto model);
        Task<ServiceResult<List<FileDriveCommentListDto>>> GetFileDriveCommentAsync(AuthenticateDto authenticate, Guid fileId, FileDriveCommentQueryDto query);
        Task<DownloadFileDto> DownloadCommentFileAsync(AuthenticateDto authenticate,long commentId, string fileSrc);
        Task<ServiceResult<List<UserMentionDto>>> GetUserMentionsAsync(AuthenticateDto authenticate, Guid fileId);
    }
}
