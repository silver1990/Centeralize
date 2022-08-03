using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.Payment
{
    public class BasePaymentDto
    {
        public string PaymentNumber { get; set; }

        [MaxLength(800)]
        public string Note { get; set; }

        public CurrencyType CurrencyType { get; set; }

        public int SupplierId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
    }
}
