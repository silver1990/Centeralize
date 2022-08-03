using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.PendingForPayment
{
    public class AddPendingForPaymentDto
    {
        public decimal RequestAmount { get; set; }
        public TermsOfPaymentStep PaymentStep { get; set; }
    }
}
