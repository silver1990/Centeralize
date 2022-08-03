using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Invoice;
using Raybod.SCM.Services.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IInvoiceService
    {
        Task<ServiceResult<List<WaitingReceiptAndForInvoiceListDto>>> GetWaitingReceiptOrReceiptRejectForInvoiceAsync(AuthenticateDto authenticate, WaitingForInvoiceQueryDto query);

        Task<ServiceResult<WaitingReceiptAndReceiptRejectForInvoiceInfoDto>> GetReceiptByIdForAddNewInvoiceAsync(AuthenticateDto authenticate, long receiptId);

        Task<ServiceResult<WaitingReceiptAndReceiptRejectForInvoiceInfoDto>> GetReceiptRejectByIdForAddNewInvoiceAsync(AuthenticateDto authenticate, long warehouseTransferenceId);

       

        Task<ServiceResult<bool>> AddInvoiceByReceiptAsync(AuthenticateDto authenticate,long receiptId, AddInvoiceDto model);

        Task<ServiceResult<bool>> AddInvoiceByReceiptRejectAsync(AuthenticateDto authenticate, long receiptId, AddInvoiceDto model);


        Task<ServiceResult<List<ListInvoiceDto>>> GetsInvoiceAsync(AuthenticateDto authenticate, InvoiceQueryDto query);

        Task<ServiceResult<InvoiceInfoDto>> GetInvoiceByIdAsync(AuthenticateDto authenticate, long invoiceId);

        Task<ServiceResult<PoInvoiceDto>> GetInvoiceByPOIdAsync(AuthenticateDto authenticate, long poId);

        Task<ServiceResult<int>> GetWaitingForAddInvoiceBadgeCountAsync(AuthenticateDto authenticate);

        Task<DownloadFileDto> DownloadInvoiceAttachmentAsync(AuthenticateDto authenticate, long invoiceId, string fileSrc);

        // for po tracking
        //Task<ServiceResult<List<ListInvoiceDto>>> GetsInvoiceByPoIdAsync(AuthenticateDto authenticate, InvoiceQueryDto query);
        //Task<ServiceResult<InvoiceInfoDto>> GetInvoiceByIdAndPoIdAsync(AuthenticateDto authenticate, long poId, long invoiceId);

    }
}
