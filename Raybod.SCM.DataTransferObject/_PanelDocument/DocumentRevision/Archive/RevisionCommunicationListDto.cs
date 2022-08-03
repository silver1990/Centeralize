using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class RevisionCommunicationListDto
    {
        public long CommunicationId { get; set; }

        public long DocumentRevisionId { get; set; }

        public string CommunicationCode { get; set; }

        public CommunicationType CommunicationType { get; set; }

        public DocumentCommunicationStatus CommunicationStatus { get; set; }

        public CompanyIssue CompanyIssue { get; set; }

        public string CompanyIssueName { get; set; }

        public long? CreateDate { get; set; }
    }
}
