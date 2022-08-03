using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class WarehouseProduct : BaseAuditEntity
    {
        [Key]
        public int WarehouseProductId { get; set; }

        public int ProductId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Inventory { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AcceptQuantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ReceiptQuantity { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }
    }
}
