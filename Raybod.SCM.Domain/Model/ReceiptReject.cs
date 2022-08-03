using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class ReceiptReject : BaseEntity
    {
        [Key]
        public long ReceiptRejectId { get; set; }

        [Required]
        //[Column(TypeName = "varchar(64)")]
        [MaxLength(64)]
        public string ReceiptRejectCode { get; set; }

        public long POId { get; set; }

        public long? InvoiceId { get; set; }

        public int? SupplierId { get; set; }

        public long? PackId { get; set; }

        public long? ReceiptId { get; set; }

        [MaxLength(800)]
        public string Note { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(POId))]
        public virtual PO PO { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier Supplier { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice Invoice { get; set; }

        [ForeignKey(nameof(ReceiptId))]
        public virtual Receipt Receipt { get; set; }

        [ForeignKey(nameof(PackId))]
        public virtual Pack Pack { get; set; }

        public virtual ICollection<ReceiptRejectSubject> ReceiptRejectSubjects { get; set; }
        public virtual ICollection<PAttachment> ReceiptRejectAttachments { get; set; }
    }
}
