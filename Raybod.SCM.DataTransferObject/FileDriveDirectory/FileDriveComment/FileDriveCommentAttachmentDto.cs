using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.FileDriveDirectory
{
    public class FileDriveCommentAttachmentDto
    {
        public long CommentAttachmentId { get; set; }
        [MaxLength(250)]
        public string FileName { get; set; }
        [MaxLength(50)]
        public string FileSrc { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
    }
}
