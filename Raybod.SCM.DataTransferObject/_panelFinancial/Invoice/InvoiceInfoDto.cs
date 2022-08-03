using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Invoice
{
    public class InvoiceInfoDto : BaseInvoiceDto
    {
        public long InvoiceId { get; set; }

        public string InvoiceNumber { get; set; }

        public string SupplierName { get; set; }

        public string SupplierCode { get; set; }
        public string DispatchCode { get; set; } = "";

        public string SupplierLogo { get; set; }
        public bool IsDispatch { get; set; }

        public List<InvoiceProductInfoDto> InvoiceProducts { get; set; }
        public List<InvoiceAttachmentDto> Attachments { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public InvoiceInfoDto()
        {
            InvoiceProducts = new List<InvoiceProductInfoDto>();
            UserAudit = new UserAuditLogDto();
            Attachments = new List<InvoiceAttachmentDto>();
        }
    }
}
