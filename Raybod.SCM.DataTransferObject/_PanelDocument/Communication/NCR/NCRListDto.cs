using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Document.Communication.NCR
{
    public class NCRListDto
    {
        public long CommunicationId { get; set; }

        public long DocumentRevisionId { get; set; }

        public string CommunicationCode { get; set; }

        public DocumentCommunicationStatus CommunicationStatus { get; set; }

        public CompanyIssue CompanyIssue { get; set; }

        public string CompanyIssueName { get; set; }

        public string DocTitle { get; set; }

        public string DocNumber { get; set; }

        public string DocumentGroupTitle { get; set; }

        public string DocumentRevisionCode { get; set; }

        public UserAuditLogDto UserAudit { get; set; }
        public string Replayer { get; set; }
        public long? ReplayDate { get; set; }
        public NCRReason NCRReason { get; set; }
        public NCRListDto()
        {
            UserAudit = new UserAuditLogDto();
        }
    }
}
