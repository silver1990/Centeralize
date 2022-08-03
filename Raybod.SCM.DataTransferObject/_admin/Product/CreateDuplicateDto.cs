using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Product
{

    public class CreateDuplicateDto
    {
       
        [Required(ErrorMessage = "الزامی می باشد")]
        [Display(Name = "درصد استفاده")]
        public decimal CoefficientUse { get; set; }


        [Display(Name = "نوع قطعه")]
        public MaterialType MaterialType { get; set; }

        [Display(Name = "نوع برنامه ریزی")]
        public bool IsRequiredMRP { get; set; }
        public AreaReadDTO Area { get; set; }
    }
}
