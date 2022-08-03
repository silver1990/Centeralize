using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //no db
    public enum SCMWorkFlow
    {
        [Display(Name = "هیچکدام")]
        None =0,
        [Display(Name = "درخواست خرید")]
        PurchaseRequest = 1,

        [Display(Name = "مدیریت خرید")]
        PurchaseManagement = 2,



    }
}
