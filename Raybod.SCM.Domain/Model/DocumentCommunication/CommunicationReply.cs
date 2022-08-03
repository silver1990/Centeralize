using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class CommunicationReply : BaseAuditEntity
    {
        [Key]
        public long CommunicationReplyId { get; set; }

        public long CommunicationQuestionId { get; set; }

        public string Description { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(CommunicationQuestionId))]
        public virtual CommunicationQuestion CommunicationQuestion { get; set; }

        public virtual ICollection<CommunicationAttachment> Attachments { get; set; }

    }
}
