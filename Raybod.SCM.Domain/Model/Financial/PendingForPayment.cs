using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class PendingForPayment : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        [Required]
        //[Column(TypeName = "varchar(64)")]
        [MaxLength(64)]
        public string PendingForPaymentNumber { get; set; }

        public long? PRContractId { get; set; }

        [MaxLength(60)]
        public string BaseContractCode { get; set; }

        public long? POId { get; set; }

        public int? SupplierId { get; set; }

        public long? POTermsOfPaymentId { get; set; }

        public long? InvoiceId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountPayed { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountRemained { get; set; }

        public POPaymentStatus Status { get; set; }

        [Required]
        public DateTime PaymentDateTime { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
        public bool? IsTax { get; set; }
        public virtual ICollection<PaymentPendingForPayment> PaymentPendingForPayments { get; set; }

        [ForeignKey(nameof(PRContractId))]
        public virtual PRContract PRContract { get; set; }

        [ForeignKey(nameof(POId))]
        public virtual PO PO { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier Supplier { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice Invoice { get; set; }

        [ForeignKey(nameof(POTermsOfPaymentId))]
        public virtual POTermsOfPayment POTermsOfPayment { get; set; }

        [ForeignKey(nameof(BaseContractCode))]
        public virtual Contract Contract { get; set; }
    }
}
