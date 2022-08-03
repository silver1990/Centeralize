using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //contract, PO
    public enum PContractType
    {
        [Display(Name = "داخلی")]
        Internal=1,
        [Display(Name = "خارجی")]
        Foreign=2
    }
}