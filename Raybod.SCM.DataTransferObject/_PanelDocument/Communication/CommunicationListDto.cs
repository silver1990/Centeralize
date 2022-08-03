using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using System.Text.Json.Serialization;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class CommunicationListDto
    {
        public long CommunicationId { get; set; }

        public long DocumentRevisionId { get; set; }

        public string CommunicationCode { get; set; }

        public string DocumentRevisionCode { get; set; }

        public CommunicationType CommunicationType { get; set; }

        public DocumentCommunicationStatus CommunicationStatus { get; set; }
        public CommunicationCommentStatus CommentStatus { get; set; }

        public CompanyIssue CompanyIssue { get; set; }

        public string CompanyIssueName { get; set; }

        public string DocTitle { get; set; }

        public string DocNumber { get; set; }

        public string DocumentGroupTitle { get; set; }

        public UserAuditLogDto UserAudit { get; set; }
        public string  Replayer { get; set; }
        public long?  ReplayDate { get; set; }


        [JsonIgnore]
        public long? CreateDate { get; set; }

        public CommunicationListDto()
        {
            UserAudit = new UserAuditLogDto();
        }
    }
}
