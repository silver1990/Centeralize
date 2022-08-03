using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class Payment : BaseEntity
    {
        public long PaymentId { get; set; }

        [Required]
        [MaxLength(64)]
        public string PaymentNumber { get; set; }

        [MaxLength(60)]
        public string ContractCode { get; set; }

        [MaxLength(800)]
        public string Note { get; set; }

        public int SupplierId { get; set; }

        public CurrencyType CurrencyType { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual ICollection<PaymentPendingForPayment> PaymentPendingForPayments { get; set; }
        public virtual ICollection<PaymentConfirmationWorkFlow> PaymentConfirmationWorkFlows { get; set; }

        public virtual ICollection<PaymentAttachment> PaymentAttachments { get; set; }

        public virtual FinancialAccount FinancialAccount { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier Supplier { get; set; }

        [ForeignKey(nameof(ContractCode))]
        public virtual Contract Contract { get; set; }

    }
}
