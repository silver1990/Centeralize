using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Invoice
{
    public class ListInvoiceDto
    {
        public long InvoiceId { get; set; }

        public string InvoiceNumber { get; set; }

        public PContractType PContractType { get; set; }

        public bool IsDispatch { get; set; }

        public long? DateCreate { get; set; }

        public string SupplierName { get; set; }

        public List<string> Products { get; set; }
    }
}
