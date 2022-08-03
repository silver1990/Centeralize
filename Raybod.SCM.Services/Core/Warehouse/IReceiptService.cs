using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Receipt;
using Raybod.SCM.DataTransferObject.ReportReceiptProduct;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IReceiptService
    {
        Task<ServiceResult<List<ListWaitingPackForReceiptDto>>> GetWaitingPackForReceiptAsync(AuthenticateDto authenticate, WaitingPackQueryDto query);

        Task<ServiceResult<ReceiptBadgeNotificationDto>> GetReceiptWaitingBadgeCountAsync(AuthenticateDto authenticate);

        Task<ServiceResult<ReceiptPackInfoDto>> GetWaitingPackInfoByIdAsync(AuthenticateDto authenticate, long packId, bool isPart, long? subjectProductId);

        Task<ServiceResult<bool>> AddReceiptForPackAsync(AuthenticateDto authenticate, long packId,  List<AddReceiptProductDto> model);

        Task<ServiceResult<List<ListReceiptDto>>> GetWaitingReceiptForAddQCListAsync(AuthenticateDto authenticate, ReceiptQueryDto query);

        Task<ServiceResult<List<ListReceiptDto>>> GetReceiptListAsync(AuthenticateDto authenticate, ReceiptQueryDto query);

        Task<ServiceResult<ReceiptInfoDto>> GetReceiptInfoByIdForAddQCAsync(AuthenticateDto authenticate, long receiptId);

        Task<ServiceResult<bool>> AddReceiptQualityControlAsync(AuthenticateDto authenticate, long receiptId, AddQCReceiptDto model);

        Task<ServiceResult<ReceiptInfoDto>> GetReceiptInfoByIdAsync(AuthenticateDto authenticate, long ReceiptId);

        Task<ServiceResult<List<ListReceiptDto>>> GetWaitingReceiptForRejectListAsync(AuthenticateDto authenticate, ReceiptQueryDto query);

        Task<ServiceResult<ReceiptRejectInfoDto>> GetWaitingReceiptForRejectInfoByReceiptIdAsync(AuthenticateDto authenticate, long receiptId);

        Task<ServiceResult<bool>> AddReceiptRejectAsync(AuthenticateDto authenticate, long receiptId, AddReceiptRejectDto model);

        Task<ServiceResult<List<ListReceiptRejectDto>>> GetReceiptRejectListAsync(AuthenticateDto authenticate, ReceiptQueryDto query);

        Task<ServiceResult<ReceiptRejectInfoDto>> GetReceiptRejectInfoByIdAsync(AuthenticateDto authenticate, long ReceiptRejectId);

        Task<ServiceResult<List<ReportReceiptProductDto>>> GetCumulativeReportReceiptProductByPoId(AuthenticateDto authenticate, long poId)
        {
            throw new System.Exception();
        }

        Task<ServiceResult<List<ReportReceiptProductDto>>> GetReportReceiptProductByPoIdAsync(AuthenticateDto authenticate, long poId);
        Task<ServiceResult<List<ReportReceiptProductDto>>> GetReportReceiptProductByPackIdAsync(AuthenticateDto authenticate, long poId, long packId);

        Task<DownloadFileDto> DownloadReceiptRejectAttachmentAsync(AuthenticateDto authenticate, long receiptRejectId, string fileSrc);

        Task<DownloadFileDto> DownloadReceiptQualityControleAttachmentAsync(AuthenticateDto authenticate, long receiptId, string fileSrc);
    }
}
