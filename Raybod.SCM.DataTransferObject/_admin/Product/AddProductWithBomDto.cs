using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Product
{
    public class AddProductWithBomDto
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


        [Display(Name = "نوع قطعه")]
        public MaterialType MaterialType { get; set; }

        [Display(Name = "نوع برنامه ریزی")]
        public bool IsRequiredMRP { get; set; }
        public AreaReadDTO Area { get; set; }
        public int? ProductId { get; set; }
        public bool IsRegisterd { get; set; }
    }
}

