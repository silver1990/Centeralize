using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class PRContractSubject : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public int ProductId { get; set; }

        public long PRContractId { get; set; }

        public long RFPItemId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ReservedStock { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RemainedStock { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DeliveredQuantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RemainedQuantityToInvoice { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }

        [ForeignKey(nameof(PRContractId))]
        public virtual PRContract PRContract { get; set; }

        [ForeignKey(nameof(RFPItemId))]
        public virtual RFPItems RFPItem { get; set; }

    }
}