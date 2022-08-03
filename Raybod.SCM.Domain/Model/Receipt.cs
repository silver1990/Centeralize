using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class Receipt : BaseEntity
    {
        [Key]
        public long ReceiptId { get; set; }

        [Required]
        [MaxLength(64)]
        public string ReceiptCode { get; set; }

        public long POId { get; set; }

        public long? InvoiceId { get; set; }

        public long? PackId { get; set; }

        public int? SupplierId { get; set; }

        [MaxLength(800)]
        public string Note { get; set; }

        public ReceiptStatus ReceiptStatus { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(POId))]
        public PO PO { get; set; }

        [ForeignKey(nameof(PackId))]
        public Pack Pack { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public Supplier Supplier { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public Invoice Invoice { get; set; }

        public QualityControl QualityControl { get; set; }

        public virtual ICollection<ReceiptSubject> ReceiptSubjects { get; set; }

        public virtual ICollection<ReceiptReject> ReceiptRejects { get; set; }

        public virtual ICollection<PAttachment> ReceiptAttachments { get; set; }

    }
}
