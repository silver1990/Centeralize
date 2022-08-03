using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Service
{
    public class AddServiceDto
    {
        [Display(Name = "کد کالا")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string ServiceCode { get; set; }

        [Display(Name = "شرح کالا")]
        [Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(400, ErrorMessage = "حداکثر مقدار فیلد {1} می باشد")]
        public string Description { get; set; }

    }
}
