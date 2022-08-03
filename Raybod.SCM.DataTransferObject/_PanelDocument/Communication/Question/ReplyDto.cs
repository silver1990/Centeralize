using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class ReplyDto
    {
        public string Description { get; set; }

        public List<CommunicationAttachmentDto> Attachments { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public ReplyDto()
        {
            Attachments = new List<CommunicationAttachmentDto>();
            UserAudit = new UserAuditLogDto();
        }
    }
}
