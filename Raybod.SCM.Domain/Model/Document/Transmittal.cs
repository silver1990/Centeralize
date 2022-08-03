using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class Transmittal : BaseAuditEntity
    {
        [Key]
        public long TransmittalId { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }

        public int DocumentGroupId { get; set; }

        public TransmittalType TransmittalType { get; set; }

        [MaxLength(64)]
        public string TransmittalNumber { get; set; }

        [MaxLength(800)]
        public string Description { get; set; }

        public int? SupplierId { get; set; }
        public int? ConsultantId { get; set; }
        [MaxLength(200)]
        public string FullName { get; set; }

        [MaxLength(300)]
        public string Email { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ContractCode))]
        public virtual Contract Contract { get; set; }

        [ForeignKey(nameof(DocumentGroupId))]
        public virtual DocumentGroup DocumentGroup { get; set; }

        [ForeignKey(nameof(ConsultantId))]
        public virtual Consultant Consultant { get; set; }
        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier Supplier { get; set; }
        public virtual ICollection<TransmittalRevision> TransmittalRevisions { get; set; }

        public virtual ICollection<RevisionAttachment> Attachments { get; set; }
    }
}
