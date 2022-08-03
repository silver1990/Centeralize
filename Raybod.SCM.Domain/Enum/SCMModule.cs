using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //no db
    public enum SCMModule
    {
        [Display(Name = "فروش")]
        Sale = 1,

        [Display(Name = "مهندسی")]
        Engineering = 2,

        [Display(Name = "برنامه ریزی")]
        Planning = 3,

        [Display(Name = "درخواست خرید")]
        PurchaseRequest = 4,

        [Display(Name = "مدیریت خرید")]
        PurchaseManagement = 5,

        [Display(Name = "انبار")]
        Warehouse = 6,

        [Display(Name = "حمل و نقل")]
        Logistic = 7,

        [Display(Name = "تنظیمات")]
        Configuration = 8
    }
}
