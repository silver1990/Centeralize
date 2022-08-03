using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Product
{
    public class EditProductDto
    {
        [Required(ErrorMessage = "الزامی می باشد")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(400, ErrorMessage = "حداکثر مقدار فیلد {1} می باشد")]
        public string ProductDescription { get; set; }

        [MaxLength(30, ErrorMessage = "حداکثر مقدار فیلد {1} می باشد")]
        public string ProductTechnicalNumber { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(100, ErrorMessage = "حداکثر مقدار فیلد {1} می باشد")]
        public string Unit { get; set; }

        public int ProductGroupId { get; set; }

    }
}
