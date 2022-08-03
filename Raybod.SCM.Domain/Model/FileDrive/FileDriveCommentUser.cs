using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class FileDriveCommentUser
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        public long CommentId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        [ForeignKey(nameof(CommentId))]
        public virtual FileDriveComment FileDriveComment { get; set; }
    }
}
