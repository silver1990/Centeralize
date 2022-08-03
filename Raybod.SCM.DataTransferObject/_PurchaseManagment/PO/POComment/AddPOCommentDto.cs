using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PO.POComment
{
    public class AddPOCommentDto
    {
        [Required]
        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        public List<AddAttachmentDto> Attachments { get; set; }

        public List<int> UserIds { get; set; }

    }
}
