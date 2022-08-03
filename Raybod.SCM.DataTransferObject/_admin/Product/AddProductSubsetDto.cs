using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Product
{ 
    public class AddProductSubsetDto
    {
        [Required(ErrorMessage = "الزامی می باشد")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(400, ErrorMessage = "حداکثر مقدار فیلد {1} می باشد")]
        public string Description { get; set; }

        [MaxLength(30, ErrorMessage = "حداکثر مقدار فیلد {1} می باشد")]
        public string TechnicalNumber { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(100, ErrorMessage = "حداکثر مقدار فیلد {1} می باشد")]
        public string Unit { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        [Display(Name = "درصد استفاده")]
        public decimal CoefficientUse { get; set; }

        public int? ProductId { get; set; }
        public bool IsRegisterd { get; set; }
    }
}