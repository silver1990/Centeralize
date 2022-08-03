using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class BasePRContractSubjectDto
    {
        [Display(Name = "محصول")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public int ProductId { get; set; }

        public long RFPId { get; set; }

        [Display(Name = "تیراژ")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public decimal Quantity { get; set; }

        [Display(Name = "قیمت واحد")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public decimal Price { get; set; }

        [Display(Name = "مبلغ کل")]
        public decimal PriceTotal
        {
            get
            {
                return Quantity * Price;
            }
        }
    }
}