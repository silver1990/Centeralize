using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class ReceiptPackInfoDto
    {
        public long PackId { get; set; }

        public string PackCode { get; set; }

        public string POCode { get; set; }

        public string PRContractCode { get; set; }

        //public int SupplierId { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierName { get; set; }
        
        public long? LogisticDateEnd { get; set; }
        

        //public string SupplierImage { get; set; }


        public List<ReceiptPackSubjectDto> ReceiptSubjects { get; set; }
    }
}
