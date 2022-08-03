using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Bom
{
    public class BomForMrpDto
    {
        public long Id { get; set; }

        [Display(Name = "کد کالا")]
        public string ProductCode { get; set; }

        [Display(Name = "شرح کالا")]
        public string ProductDescription { get; set; }

        [Display(Name = "واحد کالا")]
        public string ProductUnit { get; set; }

        [Display(Name = "شماره فنی کالا")]
        public string ProductTechnicalNumber { get; set; }

        public int ProductId { get; set; }

        public long? ParentBomId { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        [Display(Name = "درصد استفاده")]
        public decimal CoefficientUse { get; set; }

        public decimal GrossRequirement { get; set; }

        public decimal WarhouseStock { get; set; }

        public List<BomForMrpDto> ChildBoms { get; set; }

        public BomForMrpDto()
        {
            ChildBoms = new List<BomForMrpDto>();
        }

    }
}
