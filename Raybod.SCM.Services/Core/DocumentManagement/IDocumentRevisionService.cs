using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IDocumentRevisionService
    {
        Task<RevisionDashboardBadgeDto> GetRevisionDashboardBadgeAsync(AuthenticateDto authenticate);

        Task<ServiceResult<PendingRevisionBadgeCountDto>> GetPendingRevisonBadgeCountAsync(AuthenticateDto authenticate);

        Task<ServiceResult<InProgressRevisionListDto>> AddDocumentRevisionAsync(AuthenticateDto authenticate, long documentId, AddDocumentRevisionDto model);
        Task<ServiceResult<DocumentViewDto>> AddDocumentRevisionFromListAsync(AuthenticateDto authenticate, long documentId, AddDocumentRevisionDto model);
        Task<ServiceResult<List<InProgressRevisionListDto>>> GetInprogressDocumentRevisionAsync(AuthenticateDto authenticate, DocRevisionQueryDto query);

        Task<ServiceResult<bool>> DeActiveRevisionAsync(AuthenticateDto authenticate, long documentId, long revisoinId);

        Task<ServiceResult<DocumentRevisionDetailsDto>> GetDocumentRevisionByIdAsync(AuthenticateDto authenticate, long revisionId);

        Task<ServiceResult<List<RevisionAttachmentDto>>> AddDocumentRevisionAttachmentAsync(AuthenticateDto authenticate, long documentId, long revisionId, IFormFileCollection files);

        Task<ServiceResult<List<RevisionAttachmentDto>>> GetDocumentRevisionAttachmentAsync(AuthenticateDto authenticate, long documentId, long revisionId, RevisionAttachmentType type);

        Task<ServiceResult<bool>> DeleteDocumentRevisionAttachmentAsync(AuthenticateDto authenticate, long documentId, long revisionId, string fileSrc);

        Task<DownloadFileDto> DownloadRevisionFileAsync(AuthenticateDto authenticate, long docId, long revId, string fileSrc);

        Task<DownloadFileDto> DownloadRevisionFileAsync(AuthenticateDto authenticate, long revId, string fileSrc, RevisionAttachmentType type);

        Task<DownloadFileDto> DownloadRevisionFileAsync(AuthenticateDto authenticate, long revId);

        Task<DownloadFileDto> DownloadRevisionFileForCustomerUserAsync(AuthenticateDto authenticate, long revId, string fileSrc, RevisionAttachmentType type,bool accessability);

        Task<ServiceResult<List<ProductDocumentDto>>> GetDocumentByProductIdBaseOnContractAsync(AuthenticateDto authenticate, int productId);

        Task<DownloadFileDto> GetLastDocumentRevisionAttachAzZipFileByProductIdAsync(AuthenticateDto authenticate, long documentId);
        Task<DownloadFileDto> GetLastDocumentRevisionAttachAsZipFileByProductIdAsync(AuthenticateDto authenticate, long documentId, int productId);
        Task<ServiceResult<string>> ImportFileFromSharing(AuthenticateDto authenticate, long documentId, long revisionId, string fileSrc);
        Task<DownloadFileDto> GetLastDocumentRevisionsAttachAsZipFileByProductIdAsync(AuthenticateDto authenticate, int productId);
        Task<ServiceResult<List<RevisionAttachmentDto>>> GetDocumentRevisionAttachmentForCustomerUserAsync(AuthenticateDto authenticate,long documentId, long revisionId, RevisionAttachmentType type, bool accessability);
        Task<DownloadFileDto> DownloadRevisionFileForCustomerAsync(AuthenticateDto authenticate, long revId, bool accessability);

        Task<ServiceResult<bool>> EditDocumentRevisionAsync(AuthenticateDto authenticate, long documentRevisionId, EditRevisionDto model);
    }
}
