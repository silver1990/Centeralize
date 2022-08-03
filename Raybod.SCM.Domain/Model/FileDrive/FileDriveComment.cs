using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class FileDriveComment : BaseEntity
    {
        
            [Key]
            public long CommentId { get; set; }

            public Guid FileId { get; set; }

            public string Message { get; set; }

            public long? ParentCommentId { get; set; }

            [ForeignKey(nameof(ParentCommentId))]
            public FileDriveComment ParentComment { get; set; }

            [ForeignKey(nameof(FileId))]
            public virtual FileDriveFile File { get; set; }


            public virtual ICollection<FileDriveComment> ReplayComments{ get; set; }

            public virtual ICollection<FileDriveCommentUser> CommentUsers { get; set; }

            public virtual ICollection<FileDriveCommentAttachment> Attachments { get; set; }
        
    }
}
