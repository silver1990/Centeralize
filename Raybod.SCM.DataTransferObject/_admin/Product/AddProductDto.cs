using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Product
{
    public class AddProductDto
    {
        [Required(ErrorMessage = "الزامی می باشد")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(400, ErrorMessage = "حداکثر مقدار فیلد {1} می باشد")]
        public string Description { get; set; }

        [MaxLength(50, ErrorMessage = "حداکثر مقدار فیلد {1} می باشد")]
        public string TechnicalNumber { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(100, ErrorMessage = "حداکثر مقدار فیلد {1} می باشد")]
        public string Unit { get; set; }

        public ProductType ProductType { get; set; }

    }
}
