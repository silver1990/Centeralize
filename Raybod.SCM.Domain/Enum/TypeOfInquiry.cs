using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //PurchasRequest
    public enum TypeOfInquiry
    {
        [Display(Name = "فنی و بازرگانی")]
        TCRFP = 1,

        [Display(Name = "بازرگانی")]
        CRFP = 2,

        [Display(Name = "ترک تشریفات - قرارداد خرید مستقیم")]
        ContractType = 3,

        [Display(Name = "ترک تشریفات - سفارش مستقیم")]
        POType = 4,
    }
}
