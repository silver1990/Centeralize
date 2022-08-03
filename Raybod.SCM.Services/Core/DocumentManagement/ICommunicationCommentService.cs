using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Document.Communication;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface ICommunicationCommentService
    {
        Task<ServiceResult<bool>> AddCommunicationCommentAsync(AuthenticateDto authenticate, long revisionId, AddCommunicationCommentDto model);

        Task<ServiceResult<List<CommunicationListDto>>> GetCommunicationCommentListAsync(AuthenticateDto authenticate, COMQueryDto query);
        Task<ServiceResult<List<CommunicationListDto>>> GetCommunicationCommentListForCustomerUserAsync(AuthenticateDto authenticate, COMQueryDto query,bool accessability);
        Task<ServiceResult<CommnetQuestionDetailsDto>> GetCommentQuestionDetailsAsync(AuthenticateDto authenticate, long communicationId);
        Task<ServiceResult<CommnetQuestionDetailsDto>> GetCommentQuestionDetailsForCustomerUserAsync(AuthenticateDto authenticate, long communicationId,bool accessability);
        Task<ServiceResult<CommentDetailsDto>> GetCommentDetailsAsync(AuthenticateDto authenticate, long communicationId);

        Task<ServiceResult<CommnetQuestionDetailsDto>> AddReplyCommentAsync(AuthenticateDto authenticate, long communicationId, ReplyCommunicationCommentDto model);

        Task<ServiceResult<List<CommunicationAttachmentDto>>> AddCommunicationCommentAttachmentAsync(AuthenticateDto authenticate,
            long communicationId, IFormFileCollection files);

        Task<ServiceResult<bool>> DeleteCommunicationCommentAttachmentAsync(AuthenticateDto authenticate, long communicationId, string fileSrc);
        Task<DownloadFileDto> GenerateCommentPdfAsync(AuthenticateDto authenticate, long communicationId);
        Task<DownloadFileDto> DownloadCommentFileAsync(AuthenticateDto authenticate, long communicationId, string fileSrc);

        Task<DownloadFileDto> GenerateCommentAttachmentZipAsync(AuthenticateDto authenticate, long communicationId);
        Task<ServiceResult<bool>> AddCommunicationCommentForCustomerUserAsync(AuthenticateDto authenticate, long revisionId, AddCommunicationCommentDto model, bool accessability);
    }
}
