using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.PendingForPayment
{
    public class PendingForPaymentForPayedDto
    {
        public long PendingForPaymentId { get; set; }

        public string PendingForPaymentNumber { get; set; }

        public string POCode { get; set; }

        public string InvoiceNumber { get; set; }

        public string PRContractCode { get; set; }

        public long PaymentDateTime { get; set; }

        public TermsOfPaymentStep PaymentStep { get; set; }

        public CurrencyType CurrencyType { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountPayed { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountRemained { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PaymentAmount { get; set; }

    }
}
