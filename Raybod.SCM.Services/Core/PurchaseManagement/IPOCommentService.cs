using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.PO.POComment;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IPOCommentService
    {
        Task<ServiceResult<string>> AddPOCommentAsync(AuthenticateDto authenticate,long poId, PoCommentType commentType, AddPOCommentDto model);

        Task<ServiceResult<List<POCommentListDto>>> GetPOCommentAsync(AuthenticateDto authenticate,long poId,PoCommentType commentType, POCommentQueryDto query);

        Task<ServiceResult<List<UserMentionDto>>> GetUserMentionsAsync(AuthenticateDto authenticate, long poId);

        Task<DownloadFileDto> DownloadPOCommentAttachmentAsync(AuthenticateDto authenticate, long poId, long commentId,PoCommentType commentType ,string fileSrc);
    }
}
