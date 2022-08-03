using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IRevisionCommentService
    {
        Task<ServiceResult<string>> AddRevisionCommentAsync(AuthenticateDto authenticate, long documentId,
            long revisonId, AddRevisionCommentDto model);

        Task<ServiceResult<List<RevisionCommentListDto>>> GetRevisionCommentAsync(AuthenticateDto authenticate,
            long documentId, long revisonId, RevisionCommentQueryDto query);

        //Task<ServiceResult<bool>> RemoveRFPCommentByIdAsync(AuthenticateDto authenticate, long documentId, long revisionId, long revisionCommentId);

        Task<DownloadFileDto> DownloadCommentFileAsync(AuthenticateDto authenticate, long documentId, long revisionId, long commentId, string fileSrc);
    }
}
