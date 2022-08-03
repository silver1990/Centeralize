using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class AddProFromaCommentDto
    {
        [Required]
        public string Message { get; set; }

        public long? ParentCommentId { get; set; }


        public List<AddAttachmentDto> Attachments { get; set; }

        public List<int> UserIds { get; set; }

    }
}
