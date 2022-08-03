using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class ReportTransmittalListDto
    {
        public long TransmittalId { get; set; }

        public TransmittalType TransmittalType { get; set; }

        public string TransmittalNumber { get; set; }

        public string CompanyReceiver { get; set; }

        public string Description { get; set; }

        public int? SupplierId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public int RevisionCount { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public RevisionAttachmentDto Attachment { get; set; }

        public ReportTransmittalListDto()
        {
            UserAudit = new UserAuditLogDto();
            Attachment = new RevisionAttachmentDto();
        }
    }
}
