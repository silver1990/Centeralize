using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.OperationComment;
using Raybod.SCM.Services.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IOperationCommentService
    {
        Task<ServiceResult<string>> AddOperationCommentAsync(AuthenticateDto authenticate, Guid operationId,
             AddOperationCommentDto model);

        Task<ServiceResult<List<OperationCommentListDto>>> GetOperationCommentAsync(AuthenticateDto authenticate,
            Guid operationId, OperationCommentQueryDto query);

        //Task<ServiceResult<bool>> RemoveRFPCommentByIdAsync(AuthenticateDto authenticate, long documentId, long revisionId, long revisionCommentId);

        Task<DownloadFileDto> DownloadCommentFileAsync(AuthenticateDto authenticate, Guid operationId, long commentId, string fileSrc);
    }
}
