using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.Domain.Model
{
    public class POTermsOfPayment : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public long? PRContractId { get; set; }

        public long? POId { get; set; }

        public TermsOfPaymentStep PaymentStep { get; set; }

        public POPaymentStatus PaymentStatus { get; set; }

        [Required]
        public bool IsCreditPayment { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PaymentPercentage { get; set; }

        public int CreditDuration { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(PRContractId))]
        public virtual PRContract PRContract { get; set; }

        [ForeignKey(nameof(POId))] public virtual PO PO { get; set; }
    }
}