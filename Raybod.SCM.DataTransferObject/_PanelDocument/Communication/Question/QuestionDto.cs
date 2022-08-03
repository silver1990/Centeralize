using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class QuestionDto
    {
        public long QuestionId { get; set; }

        public string Description { get; set; }

        public bool IsReplyed { get; set; }

        public List<CommunicationAttachmentDto> Attachments { get; set; }

        public ReplyDto Reply { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public QuestionDto()
        {
            Reply = new ReplyDto();
            Attachments = new List<CommunicationAttachmentDto>();
            UserAudit = new UserAuditLogDto();
        }

    }
}
