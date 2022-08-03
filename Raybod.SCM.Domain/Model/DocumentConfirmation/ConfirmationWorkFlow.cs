using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class ConfirmationWorkFlow : BaseEntity
    {
        [Key]
        public long ConfirmationWorkFlowId { get; set; }

        public long? DocumentRevisionId { get; set; }

        [MaxLength(800)]
        public string ConfirmNote { get; set; }

        public int? RevisionPageNumber { get; set; }

        [MaxLength(10)]
        public string RevisionPageSize { get; set; }

        public ConfirmationWorkFlowStatus Status { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(DocumentRevisionId))]
        public virtual DocumentRevision DocumentRevision { get; set; }

        public virtual ICollection<RevisionAttachment> ConfirmationAttachments { get; set; }

        public virtual ICollection<ConfirmationWorkFlowUser> ConfirmationWorkFlowUsers { get; set; }
    }
}
