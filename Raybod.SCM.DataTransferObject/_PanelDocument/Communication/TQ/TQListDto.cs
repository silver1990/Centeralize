using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Document.Communication.TQ
{
    public class TQListDto
    {
        public long CommunicationId { get; set; }

        public long DocumentRevisionId { get; set; }

        public string CommunicationCode { get; set; }

        public DocumentCommunicationStatus CommunicationStatus { get; set; }

        public CompanyIssue CompanyIssue { get; set; }

        public string CompanyIssueName { get; set; }

        public string DocumentRevisionCode { get; set; }

        public string DocTitle { get; set; }

        public string DocNumber { get; set; }

        public string DocumentGroupTitle { get; set; }

        public UserAuditLogDto UserAudit { get; set; }
        public long? CreateDate { get; set; }
        public string Replayer { get; set; }
        public long? ReplayDate { get; set; }
        public string Subject { get; set; }
        public TQListDto()
        {
            UserAudit = new UserAuditLogDto();
        }
    }
}
