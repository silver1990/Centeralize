using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class CommunicationQuestion : BaseAuditEntity
    {
        [Key]
        public long CommunicationQuestionId { get; set; }

        public long? ParentQuestionId { get; set; }

        public long? DocumentCommunicationId { get; set; }

        public long? DocumentTQNCRId { get; set; }

        public string Description { get; set; }

        public bool IsReplyed { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(DocumentCommunicationId))]
        public virtual DocumentCommunication DocumentCommunication { get; set; }

        [ForeignKey(nameof(ParentQuestionId))]
        public virtual CommunicationQuestion ParentQuestion { get; set; }

        [ForeignKey(nameof(DocumentTQNCRId))]
        public virtual DocumentTQNCR DocumentTQNCR { get; set; }

        public virtual ICollection<CommunicationQuestion> ChildQuestions { get; set; }

        public virtual ICollection<CommunicationAttachment> Attachments { get; set; }

        public virtual CommunicationReply CommunicationReply { get; set; }

    }
}
