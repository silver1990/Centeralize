using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //PRContract
    public enum PRContractStatus
    {
        [Display(Name = "درانتظار ثبت")]
        Register = 1,

        [Display(Name = "فعال")]
        Active = 2,

        [Display(Name = "رد شده")]
        Rejected = 3,

        [Display(Name = "خاتمه یافته")]
        Compeleted = 4,

        [Display(Name = "لغو شده")]
        Canceled = 5
    }
}