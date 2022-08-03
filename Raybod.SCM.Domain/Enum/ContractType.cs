using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //Contract
    public enum ContractType
    {
     
        [Display(Name = "اصل")]
        Genuine = 1,

        [Display(Name = "الحاقیه")]
        Addendum = 2,
    }
}
