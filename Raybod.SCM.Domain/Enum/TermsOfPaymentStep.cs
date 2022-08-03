using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //POTermsOfPayment
    public enum TermsOfPaymentStep
    {
        /// <summary>
        /// پیش پرداخت هنگام ثبت سفارش
        /// </summary>
        [Display(Name = "تایید سفارش")] ApprovedPo = 1,

        /// <summary>
        /// در حال آماده سازی
        /// </summary>
        [Display(Name = "در حال آماده سازی")] Preparation = 2,

        /// <summary>
        /// عملیات پیش از حمل
        /// </summary>
        [Display(Name = "عملیات پیش از حمل")] packing = 3,

        /// <summary>
        /// صدور فاکتور
        /// </summary>
        [Display(Name = "صدور فاکتور")]
        InvoiceIssue = 4,

        /// <summary>
        /// مالیات
        /// </summary>
        [Display(Name = "مالیات")]
        Tax = 5,
    }
}
