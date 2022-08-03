using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //BOMProduct
    public enum MaterialType
    {
        [Display(Name = "مواد اولیه")]
        RawMaterial = 1,

        [Display(Name = "مجموعه")]
        Component = 2,

        [Display(Name = "قطعه")]
        Part = 3
    }
}
