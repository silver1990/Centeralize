using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication;
using Raybod.SCM.DataTransferObject._PanelDocument.Document;
using Raybod.SCM.DataTransferObject._PanelDocument.DocumentRevision.Archive;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IDocumentService
    {


        Task<ServiceResult<List<DocumentLogDto>>> GetDocumentLogAsync(AuthenticateDto authenticate, long documentId);

        Task<ServiceResult<List<DocumentViewDto>>> AddDocumentAsync(AuthenticateDto authenticate, int documentGroupId, List<AddListDocumentDto> model);
      

        Task<ServiceResult<List<DocumentViewDto>>> GetDocumentAsync(AuthenticateDto authenticate, DocumentQueryDto query,bool? isRequireTransmittal,bool? type);

        Task<ServiceResult<List<DocumentMiniOnfoDto>>> GetActiveForAddRevisionDocumentAsync(AuthenticateDto authenticate, DocumentQueryDto query);

        Task<ServiceResult<DocumentDetailsDto>> GetDocumentByIdAsync(AuthenticateDto authenticate, long documentId);

        Task<ServiceResult<bool>> ChangeActiveStateOfDocumentAsync(AuthenticateDto authenticate, long documentId);

        Task<ServiceResult<bool>> EditDocumentByDocumentIdAsync(AuthenticateDto authenticate, long documentId, AddDocumentDto model);

        Task<DownloadFileDto> ExportDocumentListAsync(AuthenticateDto authenticate);
        Task<DownloadFileDto> ExportDocumentsHistoryAsync(AuthenticateDto authenticate);
        Task<DownloadFileDto> ExportDocumentsRevisionHistoryExcelAsync(AuthenticateDto authenticate);

        Task<DownloadFileDto> DownloadImportTemplateAsync(AuthenticateDto authenticate);

        Task<ServiceResult<List<AddDocumentDto>>> ReadImportDocumentExcelFileAsync(AuthenticateDto authenticate, IFormFile formFile);

        Task<ServiceResult<DocumentArchiveInfoDto>> GetDocumentArchiveAsync(AuthenticateDto authenticate, long documentId);

        Task<ServiceResult<List<RevisionMiniInfoDto>>> GetLastTransmittalRevisionAsync(AuthenticateDto authenticate, DocumentQueryDto query);

        Task<ServiceResult<RevisionMiniInfoForCusomerUserDto>> GetLastTransmittalRevisionForDocumentAsync(AuthenticateDto authenticate, long revisionId, bool accessability);

        Task<ServiceResult<List<PendingDocumentForCommentDto>>> GetPenndingDocumentForCommentsAsync(AuthenticateDto authenticate, DocumentQueryDto query, bool? isRequireTransmittal, bool? type,bool accessability);

        Task<ServiceResult<List<RevisionMiniInfoDto>>> GetLastTransmittalRevisionForCustomerAsync(AuthenticateDto authenticate, DocumentQueryDto query,bool accessablity);

        Task<ServiceResult<List<DocumentViewDto>>> GetDocumentForCustomerUserAsync(AuthenticateDto authenticate, DocumentQueryDto query, bool? isRequireTransmittal, bool? type, bool accessability);

        Task<ServiceResult<DocumentArchiveInfoForCustomerUserDto>> GetDocumentArchiveForCustomerUserAsync(AuthenticateDto authenticate, long documentId, bool accessability);
        Task<DownloadFileDto> ExportDocumentListForCustomerUserAsync(AuthenticateDto authenticate,bool accessability);
        Task<DownloadFileDto> ExportDocumentsHistoryForCustomerUserAsync(AuthenticateDto authenticate, bool accessability);
        Task<DownloadFileDto> ExportDocumentsRevisionHistoryExcelForCustomerUserAsync(AuthenticateDto authenticate, bool accessability);

    }
}
