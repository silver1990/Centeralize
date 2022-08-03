using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class ReceiptRejectSubject
    {
        [Key]
        public long ReceiptRejectSubjectId { get; set; }

        public int ProductId { get; set; }

        public long? ReceiptRejectId { get; set; }

        public long? ParentSubjectId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ReceiptQuantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        [ForeignKey(nameof(ReceiptRejectId))]
        public ReceiptReject ReceiptReject { get; set; }


        [ForeignKey(nameof(ParentSubjectId))]
        public virtual ReceiptRejectSubject ParentSubject { get; set; }

        public virtual ICollection<ReceiptRejectSubject> ReceiptRejectSubjectPartLists { get; set; }
    }
}
