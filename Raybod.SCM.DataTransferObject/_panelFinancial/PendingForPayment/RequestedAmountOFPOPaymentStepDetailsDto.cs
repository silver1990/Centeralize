using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.PendingForPayment
{
    public class RequestedAmountOFPOPaymentStepDetailsDto
    {
        public decimal POTotalAmount { get; set; }

        public decimal PaymentStepAmount { get; set; }

        public decimal BeforeRequestedAmount { get; set; }

        public decimal RemainedPaymentStepAmount { get; set; }

        public decimal RequestedAmount { get; set; }
    }

    public class RequestedAmountOFPOPaymentStepDto
    {
        public TermsOfPaymentStep PaymentStep { get; set; }
        public decimal RemainedPaymentStepAmount { get; set; }
    }
}
