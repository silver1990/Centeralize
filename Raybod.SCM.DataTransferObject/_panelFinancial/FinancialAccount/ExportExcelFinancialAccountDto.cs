using Raybod.SCM.Domain.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.FinancialAccount
{
    public class ExportExcelFinancialAccountDto
    {
        [Description("ردیف")]
        public int RowNumber { get; set; }

        [Description("نام تامین کننده")]
        public string Name { get; set; }
        [Description("کد تامین کننده")]
        public string Code { get; set; }

        [Description("واحد پولی")]
        public string CurrencyType { get; set; }

        [Description("خرید")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PurchaseAmount { get; set; }

        [Description("برگشت از خرید")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RejectPurchaseAmount { get; set; }

        [Description("پرداخت")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PaymentAmount { get; set; }

        [Description("مانده")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RemainedAmount { get; set; }
        public CurrencyType Currency { get; set; }
    }
}
