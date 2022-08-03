using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.View
{
    public class FinancialAccountBaseOnSupplier
    {
  
        public string Name { get; set; }
                
        public string Logo { get; set; }

        public int Value { get; set; }
        public string Label { get; set; }
        public CurrencyType CurrencyType { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal InitialAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PurchaseAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RejectPurchaseAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PaymentAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RemainedAmount { get; set; }
    }
}
