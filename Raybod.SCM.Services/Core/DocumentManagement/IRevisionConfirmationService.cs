using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.Document.RevisionConfirmation;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IRevisionConfirmationService
    {
        Task<ServiceResult<List<UserMentionDto>>> GetConfirmationUserListAsync(AuthenticateDto authenticate, long revisionId);

        Task<ServiceResult<bool>> SetConfirmationRevisionAsync(AuthenticateDto authenticate,
                       long docId, long revId, AddRevisionConfirmationDto model);

        Task<ServiceResult<List<PendingConfirmRevisionListDto>>> GetPendingConfirmRevisionAsync(AuthenticateDto authenticate, DocRevisionQueryDto query);

        Task<ServiceResult<ConfirmationWorkflowDto>> GetPendingConfirmRevisionByRevIdAsync(AuthenticateDto authenticate, long revisionId);

        Task<ServiceResult<bool>> SetUserConfirmOwnRevisionTaskAsync(AuthenticateDto authenticate, long revisionId, AddConfirmationAnswerDto model);

        Task<DownloadFileDto> DownloadRevisionNativeAndFinalFileAsync(AuthenticateDto authenticate, long docId, long revId, RevisionAttachmentType attachType, string fileSrc);

        Task<ServiceResult<ReportConfirmationWorkflowDto>> GetReportConfiemRevisionByRevIdAsync(AuthenticateDto authenticate, long revisionId);
        Task<ServiceResult<ReportConfirmationWorkflowDto>> GetReportConfiemRevisionByRevIdForCustomerUserAsync(AuthenticateDto authenticate, long revisionId,bool accessability);

        Task<ServiceResult<RejectedUserDto>> GetRejectedUserInfoConfirmationRevisionAsync(AuthenticateDto authenticate, long revisionId);

        Task<ServiceResult<List<ReportConfirmationWorkflowDto>>> GetReportRevisionConfirmationWorkFlowByRevIdAsync(AuthenticateDto authenticate, long revisionId);

        Task<ServiceResult<List<ListConfirmationUserWorkFlowDto>>> GetReportRevisionConfirmationWorkFlowUserByRevIdAsync(AuthenticateDto authenticate, long revisionId);
    }
}
