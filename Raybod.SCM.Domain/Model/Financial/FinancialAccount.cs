using Raybod.SCM.Domain.Enum;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class FinancialAccount
    {
        [Key]
        public long Id { get; set; }

        public int SupplierId { get; set; }

        public long? POId { get; set; }

        public long? InvoiceId { get; set; }

        public long? PaymentId { get; set; }

        public CurrencyType CurrencyType { get; set; }

        public FinancialAccountType FinancialAccountType { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PurchaseAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RejectPurchaseAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PaymentAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RemainedAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal InitialAccount { get; set; }

        [Required]
        public DateTime DateDone { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier Supplier { get; set; }

        [ForeignKey(nameof(POId))]
        public virtual PO PO { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice Invoice { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public virtual Payment Payment { get; set; }
    }
}
