using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.FileDriveDirectory
{
    public class AddFileDriveCommentDto
    {
        [Required]
        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        public List<FileDriveCommentAttachmentDto> Attachments { get; set; }

        public List<int> UserIds { get; set; }
    }
}
