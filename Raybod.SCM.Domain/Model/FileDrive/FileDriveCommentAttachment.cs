using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class FileDriveCommentAttachment:BaseEntity
    {
        [Key]
        public long CommentAttachmentId { get; set; }

        public long? FileDriveCommentId { get; set; }

        [MaxLength(250)]
        public string FileName { get; set; }

        [MaxLength(250)]
        public string FileSrc { get; set; }

        [Required]
        public long FileSize { get; set; }

        [Required]
        public string FileType { get; set; }

        [ForeignKey(nameof(FileDriveCommentId))]
        public virtual FileDriveComment FileDriveComment { get; set; }

      
    }
}
