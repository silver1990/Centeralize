﻿using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Bom
{
    public class BaseBomDto
    {
        public int ProductId { get; set; }

        public long? ParentBomId { get; set; }
        public int? AreaId { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        [Display(Name = "درصد استفاده")]
        public decimal CoefficientUse { get; set; }

        [Display(Name = "نوع قطعه")]
        public MaterialType MaterialType { get; set; }



    }
}
