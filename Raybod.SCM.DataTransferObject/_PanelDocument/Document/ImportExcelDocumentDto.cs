using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class ImportExcelDocumentDto
    {
        //public long DocumentId { get; set; }

        //public string ContractCode { get; set; }

        //public int DocumentGroupId { get; set; }

        //public bool IsActive { get; set; }

        public string DocNumber { get; set; }

        public string ClientDocNumber { get; set; }

        public string DocTitle { get; set; }

        public string DocRemark { get; set; }

        public DocumentClass DocClass { get; set; }


    }
}
