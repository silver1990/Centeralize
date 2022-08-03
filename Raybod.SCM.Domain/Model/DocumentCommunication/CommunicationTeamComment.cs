using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class CommunicationTeamComment : BaseEntity
    {
        [Key]
        public long CommunicationTeamCommentId { get; set; }

        public long? DocumentCommunicationId { get; set; }

        public long? DocumentTQNCRId { get; set; }

        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        [ForeignKey(nameof(ParentCommentId))]
        public CommunicationTeamComment ParentComment { get; set; }

        [ForeignKey(nameof(DocumentCommunicationId))]
        public virtual DocumentCommunication Communication { get; set; }

        [ForeignKey(nameof(DocumentTQNCRId))]
        public virtual DocumentTQNCR DocumentTQNCR { get; set; }

        public virtual ICollection<CommunicationTeamComment> ReplayComments { get; set; }

        public virtual ICollection<CommunicationTeamCommentUser> CommentUsers { get; set; }

        public virtual ICollection<CommunicationAttachment> Attachments { get; set; }
    }
}
