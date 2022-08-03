using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class OperationComment:BaseEntity
    {
        [Key]
        public long OperationCommentId { get; set; }

        public Guid OperationId { get; set; }

        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        [ForeignKey(nameof(ParentCommentId))]
        public OperationComment ParentComment { get; set; }

        [ForeignKey(nameof(OperationId))]
        public virtual Operation Operation { get; set; }

        public virtual ICollection<OperationComment> ReplayComments { get; set; }

        public virtual ICollection<OperationCommentUser> OperationCommentUsers { get; set; }

        public virtual ICollection<OperationAttachment> Attachments { get; set; }
    }
}
