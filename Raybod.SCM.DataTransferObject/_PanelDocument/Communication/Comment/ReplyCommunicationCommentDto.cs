using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class ReplyCommunicationCommentDto
    {
        public List<AddReplyCommentDto> Replys { get; set; }

        public List<AddRevisionAttachmentDto> Attachments { get; set; }
    }
}
