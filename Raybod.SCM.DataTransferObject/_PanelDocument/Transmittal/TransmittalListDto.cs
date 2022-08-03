using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Transmittal
{
    public class TransmittalListDto
    {
        public long TransmittalId { get; set; }

        public TransmittalType TransmittalType { get; set; }

        public string DocumentGroupTitle { get; set; }

        public string TransmittalNumber { get; set; }

        public string CompanyReceiver { get; set; }

        public string Description { get; set; }

        public int? SupplierId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public int RevisionCount { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public RevisionAttachmentDto Attachment { get; set; }

        public TransmittalListDto()
        {
            UserAudit = new UserAuditLogDto();
            Attachment = new RevisionAttachmentDto();
        }
    }
}
