using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class DocumentRevisionListDto
    {
        public long DocumentId { get; set; }

        public long DocumentRevisionId { get; set; }

        public string DocNumber { get; set; }

        public string DocTitle { get; set; }

        public DocumentClass DocClass { get; set; }

        public string DocumentGroupTitle { get; set; }

        public string DocumentGroupCode { get; set; }

        public string DocumentRevisionCode { get; set; }

        public string Description { get; set; }

        public RevisionStatus RevisionStatus { get; set; }

        public long? DateEnd { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public DocumentRevisionListDto()
        {
            UserAudit = new UserAuditLogDto();
        }


    }
}
