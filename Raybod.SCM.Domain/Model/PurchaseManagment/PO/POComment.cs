using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class POComment : BaseEntity
    {
        [Key]
        public long POCommentId { get; set; }

        public long POId { get; set; }


        public string Message { get; set; }
        public PoCommentType CommentType { get; set; }

        public long? ParentCommentId { get; set; }

        [ForeignKey(nameof(ParentCommentId))]
        public POComment ParentComment { get; set; }

        [ForeignKey(nameof(POId))]
        public virtual PO PO { get; set; }



        public virtual ICollection<POComment> ReplayComments { get; set; }

        public virtual ICollection<POCommentUser> CommentUsers { get; set; }

        public virtual ICollection<PAttachment> Attachments { get; set; }
    }

}
