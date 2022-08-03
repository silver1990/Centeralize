using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Invoice
{
    public class WaitingReceiptAndReceiptRejectForInvoiceInfoDto : BaseInvoiceDto
    {
        //public string ReferenceCode { get; set; }

        //public string ReferenceCreatedDate { get; set; }

        public WaitingForInvoiceType WaitingForInvoiceType { get; set; }

        public string POCode { get; set; }

        public long POId { get; set; }
        public string DispatchCode { get; set; } = "";

        public string PRContractCode { get; set; }

        public string SupplierName { get; set; }

        public string  ReceiptCode { get; set; }

        public List<InvoiceProductInfoDto> InvoiceProducts { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public WaitingReceiptAndReceiptRejectForInvoiceInfoDto()
        {
            InvoiceProducts = new List<InvoiceProductInfoDto>();
            UserAudit = new UserAuditLogDto();
            
        }
    }

}
