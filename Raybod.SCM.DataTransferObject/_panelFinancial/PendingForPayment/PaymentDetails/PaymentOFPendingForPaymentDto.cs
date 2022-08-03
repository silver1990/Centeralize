using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.PendingForPayment
{
    public class PaymentOFPendingForPaymentDto
    {
        /// <summary>
        /// مبلغ پرداخت
        /// </summary>
        public long PaymentId { get; set; }
        public string PaymentNumber { get; set; }
        public decimal PaymentAmount { get; set; }

        public long? PaymentDate { get; set; }

        public string CreatedUserName { get; set; }

    }
}
