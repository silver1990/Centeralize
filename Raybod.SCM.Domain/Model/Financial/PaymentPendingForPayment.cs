using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class PaymentPendingForPayment
    {
        public long Id { get; set; }

        public long PaymentId { get; set; }

        public long PendingForPaymentId { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PaymentAmount { get; set; }
        
        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; }

        [ForeignKey(nameof(PendingForPaymentId))]
        public PendingForPayment PendingForPayment { get; set; }
    }
}
