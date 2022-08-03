using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class ContractSubjectsInfoDto
    {
        public int Id { get; set; }

        [Display(Name = "کد قرارداد")]
        public string ContractCode { get; set; }

        [Display(Name = "شناسه محصول")]
        public int ProductId { get; set; }

        [Display(Name = "کد محصول")]
        public string ProductCode { get; set; }

        [Display(Name = "نام محصول")]
        public string ProductName { get; set; }

        [Display(Name = "واحد اصلی")]
        public string ProductUnit { get; set; }

        [Display(Name = "ضریب افزایش یا کاهش")]
        public decimal BalanceingRate { get; set; }

        [Display(Name = "میزان")]
        public decimal Quantity { get; set; }

        [Display(Name = "قیمت واحد")]
        public decimal PriceUnit { get; set; }

        [Display(Name = "مبلغ کل")]
        public decimal PriceTotal { get; set; }

    }
}
