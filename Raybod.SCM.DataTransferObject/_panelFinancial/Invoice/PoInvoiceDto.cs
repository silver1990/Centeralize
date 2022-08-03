using Raybod.SCM.DataTransferObject.PO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Invoice
{
    public class PoInvoiceDto
    {
        public List<InvoiceInfoDto> InvoiceInfo { get; set; }
        public POFinancialDetailsDto PreInvoice { get; set; }
    }
}
