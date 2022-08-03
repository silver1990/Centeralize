using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.Communication.TQ
{
    public class AddTQReplyDto
    {
        public long QuestionId { get; set; }

        public string Description { get; set; }

        public List<AddRevisionAttachmentDto> Attachments { get; set; }
    }
}
