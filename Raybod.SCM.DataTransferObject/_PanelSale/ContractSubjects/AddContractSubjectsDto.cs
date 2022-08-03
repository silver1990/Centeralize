using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class AddContractSubjectsDto
    {
        [Display(Name = "محصول")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public int ProductId { get; set; }

        [Display(Name = "ضریب افزایش یا کاهش")]
        public decimal BalanceingRate { get; set; }

        [Display(Name = "میزان")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public decimal Quantity { get; set; }

        [Display(Name = "قیمت واحد")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public decimal PriceUnit { get; set; }

        [Display(Name = "مبلغ کل")]
        public decimal PriceTotal
        {
            get
            {
                return Quantity * PriceUnit;
            }
        }

    }
}
