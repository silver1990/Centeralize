using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class RevisionArchiveDto
    {
        public long DocumentRevisionId { get; set; }

        public string DocumentRevisionCode { get; set; }

        public string Description { get; set; }

        public RevisionStatus RevisionStatus { get; set; }

        public int ActivityUsers { get; set; }

        public int FinalAttachment { get; set; }

        public int NativeAttachment { get; set; }

        public ArchiveConfirmationWorkFlowDto ConfirmWorkFlow { get; set; }

        public long? TransmittalDate { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public RevisionArchiveDto()
        {
            UserAudit = new UserAuditLogDto();
            ConfirmWorkFlow = new ArchiveConfirmationWorkFlowDto();
        }
    }
}
