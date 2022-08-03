using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Invoice
{
    public class InvoiceProductInfoDto : BaseInvoiceProductDto
    {
        public long Id { get; set; }

        public long InvoiceId { get; set; }

        [Display(Name = "کد محصول")]
        public string ProductCode { get; set; }

        [Display(Name = "نام محصول")]
        public string ProductName { get; set; }

        [Display(Name = "واحد اصلی")]
        public string ProductUnit { get; set; }

        /// <summary>
        /// حمع کل پس از تخفیف
        /// </summary>
        //public decimal TotalAmountWithDiscountIRR { get; set; }
    }
}
