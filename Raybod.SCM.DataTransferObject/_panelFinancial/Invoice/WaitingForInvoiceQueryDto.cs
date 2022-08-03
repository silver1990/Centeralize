using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Invoice
{
    public class WaitingForInvoiceQueryDto : DBQueryDto
    {
        public InvoiceType InvoiceType { get; set; }
        
        public long ReferenceId { get; set; }
        
        public bool IsReceipt { get; set; }

        public List<int> ProductGroupIds { get; set; }

        public List<int> ProductIds { get; set; }

        public List<int> SupplierIds { get; set; }
    }
}
