using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //POTermsOfPayment
    public enum POPaymentStatus
    {
        /// <summary>
        /// تسویه شده
        /// </summary>
        [Display(Name = "تسویه شده")]
        NotSettled = 1,

        /// <summary>
        /// تسویه شده
        /// </summary>
        [Display(Name = "تسویه شده")]
        Settled = 2,

        /// <summary>
        /// لغو شده
        /// </summary>
        [Display(Name = "لغو شده")]
        Canceled = 3,


    }
}