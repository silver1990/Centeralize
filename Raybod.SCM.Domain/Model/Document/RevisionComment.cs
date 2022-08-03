using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RevisionComment : BaseEntity
    {
        [Key]
        public long RevisionCommentId { get; set; }

        public long DocumentRevisionId { get; set; }

        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        [ForeignKey(nameof(ParentCommentId))]
        public RevisionComment ParentComment { get; set; }

        [ForeignKey(nameof(DocumentRevisionId))]
        public virtual DocumentRevision DocumentRevision { get; set; }

        public virtual ICollection<RevisionComment> ReplayComments { get; set; }

        public virtual ICollection<RevisionCommentUser> RevisionCommentUsers { get; set; }

        public virtual ICollection<RevisionAttachment> Attachments { get; set; }
    }
}
