using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //Contract,PO,PRContract
    public enum CurrencyType
    {
        [Display(Name = "ریال")] IRR = 1,
        [Display(Name = "یورو")] EUR = 2,
        [Display(Name = "دلار")] USD = 3,
    }
}