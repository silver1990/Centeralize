using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //Contract
    public enum ContractStatus
    {
        [Display(Name = "غیر فعال")]
        DeActive = 0,

        [Display(Name = "فعال")]
        Active = 1,

        [Display(Name = "مختومه")]
        Closed = 2

    }
}
