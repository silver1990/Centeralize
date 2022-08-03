using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PO.POSupplierDocuemnt
{
    public class POSupplierDocumentProductListDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string ProductUnit { get; set; }

        public string ProductGroupName { get; set; }

        public string TechnicalNumber { get; set; }

    }
}
