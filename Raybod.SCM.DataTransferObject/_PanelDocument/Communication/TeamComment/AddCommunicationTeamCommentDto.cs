using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class AddCommunicationTeamCommentDto
    {
        [Required]
        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        public List<AddRevisionAttachmentDto> Attachments { get; set; }

        public List<int> UserIds { get; set; }

    }
}
