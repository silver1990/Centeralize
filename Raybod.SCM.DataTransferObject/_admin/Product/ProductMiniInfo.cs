using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Product
{
    public class ProductMiniInfo
    {
        public int Id { get; set; }

        [Display(Name = "کد کالا")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string ProductCode { get; set; }

        [Display(Name = "شماره فنی")]
        public string TechnicalNumber { get; set; }

        [Display(Name = "شرح کالا")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        [MaxLength(400, ErrorMessage = "حداکثر مقدار فیلد {1} می باشد")]
        public string Description { get; set; }

        [Display(Name = "واحد اصلی کالا")]
        public string Unit { get; set; }
        public string Image { get; set; }

    }
}
