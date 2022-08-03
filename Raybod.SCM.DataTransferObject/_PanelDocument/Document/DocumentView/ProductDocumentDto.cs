using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class ProductDocumentDto
    {
        public long DocumentId { get; set; }

        public string DocTitle { get; set; }
        public string DocumentGroupTitle { get; set; }

        public string DocNumber { get; set; }

        public string ClientDocNumber { get; set; }

        public DocumentClass DocClass { get; set; }

        public string DocumentRevisionCode { get; set; }

        public long? LastRevisionDate { get; set; }

        public bool HasRevision { get; set; }
    }
}
