using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class DocumentRevision : BaseEntity
    {
        [Key]
        public long DocumentRevisionId { get; set; }

        public long DocumentId { get; set; }

        [Required]
        [MaxLength(64)]
        public string DocumentRevisionCode { get; set; }

        [MaxLength(800)]
        public string Description { get; set; }

        public int? RevisionPageNumber { get; set; }

        [MaxLength(10)]
        public string RevisionPageSize { get; set; }

        public bool IsLastConfirmRevision { get; set; }

        public bool IsLastRevision { get; set; }

        public RevisionStatus RevisionStatus { get; set; }

        public DateTime? DateEnd { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        public virtual ICollection<RevisionActivity> RevisionActivities { get; set; }

        public virtual ICollection<RevisionComment> RevisionComments { get; set; }

        public virtual ICollection<RevisionAttachment> RevisionAttachments { get; set; }

        public virtual ICollection<ConfirmationWorkFlow> ConfirmationWorkFlows { get; set; }

        public virtual ICollection<TransmittalRevision> TransmittalRevisions { get; set; }

        public virtual ICollection<DocumentCommunication> DocumentCommunications { get; set; }

        public virtual ICollection<DocumentTQNCR> DocumentTQNCRs { get; set; }

    }
}
