using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.PendingForPayment
{
    public class BasePendingForPaymentDto
    {
        public long PendingForPaymentId { get; set; }

        public string PendingForPaymentNumber { get; set; }

        public long? PRContractId { get; set; }

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

        public PendingOFPeymentStatus PendingOFPeymentStatus { get; set; }

        public long PaymentDateTime { get; set; }
    }
}
