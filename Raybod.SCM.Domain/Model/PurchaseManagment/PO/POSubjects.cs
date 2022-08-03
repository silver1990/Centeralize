using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class POSubject
    {
        [Key]
        public long POSubjectId { get; set; }

        public long? POId { get; set; }

        public long? MrpItemId { get; set; }

        public long? ParentSubjectId { get; set; }

        public int ProductId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShortageQuantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CoefficientUse { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RemainedQuantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ReceiptedQuantity { get; set; }

        public POSubjectPartInvoiceStatus POSubjectPartInvoiceStatus { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PriceUnit { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PriceTotal
        {
            get
            {
                return PriceUnit * Quantity;
            }
        }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }

        [ForeignKey(nameof(POId))]
        public virtual PO PO { get; set; }


        [ForeignKey(nameof(ParentSubjectId))]
        public virtual POSubject ParentSubject { get; set; }


        [ForeignKey(nameof(MrpItemId))]
        public virtual MrpItem MrpItem { get; set; }

    }
}