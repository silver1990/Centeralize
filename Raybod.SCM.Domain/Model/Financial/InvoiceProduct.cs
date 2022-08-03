using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class InvoiceProduct
    {
        [Key]
        public long Id { get; set; }

        public long InvoiceId { get; set; }

        public int ProductId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// جمع کل نهایی
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalProductAmount { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice Invoice { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }

    }
}
