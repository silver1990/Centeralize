using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.OperationComment
{
    public class AddOperationCommentDto
    {
        [Required]
        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        public List<AddOperationAttachmentDto> Attachments { get; set; }

        public List<int> UserIds { get; set; }
    }
}
