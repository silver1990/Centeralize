using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.Invoice
{
    public class BaseInvoiceDto
    {
        /// <summary>
        /// شناسه مرجع رسید یا خروج
        /// </summary>
        public long ReceiptId { get; set; }
        
        /// <summary>
        /// نوع فاکتور
        /// </summary>
        public InvoiceType InvoiceType { get; set; }

        public PContractType PContractType { get; set; }

        public int SupplierId { get; set; }

        /// <summary>
        /// واحد پول قرارداد
        /// </summary>
        public CurrencyType CurrencyType { get; set; }

        [MaxLength(800)]
        public string Note { get; set; }        

        /// <summary>
        /// درصد مالیات
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Tax { get; set; } = 9;
       
        /// <summary>
        /// دیگر هزینه ها
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OtherCosts { get; set; }


        /// <summary>
        /// مجموع کل
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalProductAmount { get; set; }

        /// <summary>
        /// مجموع تخفیف ها
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// مجموع مالیات 
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalTax { get; set; }

        /// <summary>
        /// جمع کل فاکتور
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalInvoiceAmount { get; set; }


    }
}
