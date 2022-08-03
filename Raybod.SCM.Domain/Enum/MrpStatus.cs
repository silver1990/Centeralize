using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //MRP
    public enum MrpStatus
    {
        [Display(Name = "غیر فعال")]
        DeActive = 0,

        [Display(Name = "فعال")]
        Active = 1,

        [Display(Name = "مختومه")]
        Closed = 2,
    }
}
