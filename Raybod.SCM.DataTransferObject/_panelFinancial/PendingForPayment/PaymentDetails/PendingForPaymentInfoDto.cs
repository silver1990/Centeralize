using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.PendingForPayment
{
    public class PendingForPaymentInfoDto
    {
        public long PendingForPaymentId { get; set; }
        public string PendingForPaymentNumber { get; set; }

        public decimal Amount { get; set; }

        public PendingOFPeymentStatus PendingOFPeymentStatus { get; set; }

        public TermsOfPaymentStep PaymentStep { get; set; }

        public CurrencyType CurrencyType { get; set; }

        public UserAuditLogDto UserAudit { get; set; }
        /// <summary>
        /// پرداختی ها
        /// </summary>
        public List<PaymentOFPendingForPaymentDto> Payments { get; set; }
        public PendingForPaymentInfoDto()
        {
            UserAudit = new UserAuditLogDto();
            Payments = new List<PaymentOFPendingForPaymentDto>();
        }
    }
}
