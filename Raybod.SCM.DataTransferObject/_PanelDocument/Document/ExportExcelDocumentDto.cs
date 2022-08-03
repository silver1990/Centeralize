using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class ExportExcelDocumentDto
    {
        public string DocNumber { get; set; }

        public string ClientDocNumber { get; set; }

        public string DocTitle { get; set; }

        public string DocRemark { get; set; }

        public DocumentClass DocClass { get; set; }

        public bool IsActive { get; set; }

        public string DocumentGroupTitle { get; set; }

        public string DocumentGroupCode { get; set; }
        public string AreaTitle { get; set; }
        public string LastRevisionCode { get; set; }
        public RevisionStatus LastRevisionStatus { get; set; }
        public CommunicationCommentStatus CommentStatus { get; set; }
        public string Remark { get; set; }
    }
}
