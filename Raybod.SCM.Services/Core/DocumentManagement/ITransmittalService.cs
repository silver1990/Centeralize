using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.Email;
using Raybod.SCM.DataTransferObject.Transmittal;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface ITransmittalService
    {
        Task<ServiceResult<List<PenddingTransmittalListDto>>> GetPendingRevisionGroupByDocumentGroupIdAsync(AuthenticateDto authenticate);

        Task<ServiceResult<bool>> AddTransmittalByPendingRevisionAsync(AuthenticateDto authenticate, int documentGroupId, AddTransmittalDto model);

        Task<ServiceResult<List<PendingTransmitalRevisionInfoDo>>> GetRevisionForAddTransmittalAsync(AuthenticateDto authenticate, int documentGroupId, DocRevisionQueryDto query);

        Task<ServiceResult<bool>> AddTransmittalAsync(AuthenticateDto authenticate, int documentGroupId, AddTransmittalDto model);

        Task<ServiceResult<List<TransmittalCompanyListDto>>> GetTransmittalCompanyListAsync(AuthenticateDto authenticate);

        Task<ServiceResult<List<TransmittalUserListDto>>> GetTransmittalSupplierUserAsync(AuthenticateDto authenticate, int supplierId);
        Task<ServiceResult<List<TransmittalUserListDto>>> GetTransmittalConsultantUserAsync(AuthenticateDto authenticate, int consultantId);
        Task<ServiceResult<List<TransmittalUserListDto>>> GetTransmittalCustomerUserAsync(AuthenticateDto authenticate, int customerId);

        Task<ServiceResult<List<TransmittalUserListDto>>> GetTransmittalInternalUserAsync(AuthenticateDto authenticate);

        Task<ServiceResult<PendingTransmittalDetailsDto>> GetPendingTransmittalDetailsAsync(AuthenticateDto authenticate, int documentGroupId);

        Task<ServiceResult<List<TransmittalListDto>>> GetTransmittalListByRevisionIdAsync(AuthenticateDto authenticate, long revisionId);

        Task<ServiceResult<List<TransmittalListDto>>> GetTransmittalListAsync(AuthenticateDto authenticate, TransmittalQueryDto query,bool? type);

        Task<ServiceResult<List<TransmittalListDto>>> GetTransmittalLisForCustomerUsertAsync(AuthenticateDto authenticate, TransmittalQueryDto query, bool? type,bool accessability);

        Task<ServiceResult<List<PendingTransmitalRevisionInfoDo>>> GetTransmittalDetailsAsync(AuthenticateDto authenticate, long transmittalId);

        Task<ServiceResult<List<PendingTransmitalRevisionInfoDo>>> GetTransmittalDetailsForCustomerUserAsync(AuthenticateDto authenticate, long transmittalId, bool accessability);

        Task<DownloadFileDto> DownloadTransmitalFileAsync(AuthenticateDto authenticate, long transmittalId);

        Task<DownloadFileDto> DownloadTransmitalFileForCustomerUserAsync(AuthenticateDto authenticate, long transmittalId,bool accessability);

        Task<DownloadFileDto> DownloadTransmitalFileForCustomerUserAsync(AuthenticateDto authenticate, long transmittalId, bool accessability, RevisionAttachmentType type = RevisionAttachmentType.Final);
        Task<ServiceResult<List<TransmittalExportExcelDto>>> GetTransmittaledRevisionListForExportToExcelAsync(AuthenticateDto authenticate,bool? type);

        Task<ServiceResult<List<TransmittalExportExcelDto>>> GetTransmittaledRevisionListForExportToExcelCustomerUserlAsync(AuthenticateDto authenticate,bool? type,bool accessability);

        Task<ServiceResult<List<TransmittalListDto>>> GetTransmittalListByRevisionIdForCustomerUserAsync(AuthenticateDto authenticate, long revisionId, bool accessability);
        Task<DownloadFileDto> DownloadTransmitalFileAsync(AuthenticateDto authenticate, long revId, RevisionAttachmentType type = RevisionAttachmentType.Final);
        Task<ServiceResult<TransmittalEmailContentDto>> GetTransmittalEmailContent(AuthenticateDto authenticate, long transmittalId);

        Task<ServiceResult<bool>> SendTransmittalEmail(AuthenticateDto authenticate, long transmittalId, TransmittalEmailContentDto model);

    }
}
