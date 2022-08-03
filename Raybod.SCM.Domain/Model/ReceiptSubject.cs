using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class ReceiptSubject
    {
        [Key]
        public long ReceiptSubjectId { get; set; }

        public int ProductId { get; set; }

        public long? ReceiptId { get; set; }

        public long? ParentSubjectId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PackQuantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ReceiptQuantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal QCAcceptQuantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShortageQuantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PurchaseRejectRemainedQuantity { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        [ForeignKey(nameof(ReceiptId))]
        public Receipt Receipt { get; set; }

        [ForeignKey(nameof(ParentSubjectId))]
        public virtual ReceiptSubject ParentSubject { get; set; }

        public virtual ICollection<ReceiptSubject> ReceiptSubjectPartLists { get; set; }

    }
}
