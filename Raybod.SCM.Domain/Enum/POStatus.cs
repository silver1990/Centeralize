using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //PO
    public enum POStatus
    {
        /// <summary>
        /// لغو شده
        /// </summary>
        [Display(Name = "لغو")] Canceled = 0,

        /// <summary>
        /// در انتظار ثبت
        /// </summary>
        [Display(Name = "در انتظار ثبت")] Pending = 1,

        /// <summary>
        /// تایید شده
        /// </summary>
        [Display(Name = "تایید")] Approved = 2,

        /// <summary>
        /// در حال آماده سازی
        /// </summary>
        [Display(Name = "در حال آماده سازی")] Preparation = 3,

        /// <summary>
        /// عملیات پیش از حمل
        /// </summary>
        [Display(Name = "عملیات پیش از حمل")] packing = 4,

        /// <summary>
        /// در حال حمل به گمرک مبدا
        /// </summary>
        [Display(Name = "در حال حمل به گمرک مبدا")]
        TransportationToOriginPort = 5,

        /// <summary>
        /// کمرک مبدا
        /// </summary>
        [Display(Name = "کمرک مبدا")]
        OriginPort = 6,

        /// <summary>
        /// در حال حمل به گمرک مقصد
        /// </summary>
        [Display(Name = "در حال حمل به گمرک مقصد")]
        TransportationToDestinationPort = 7,

        /// <summary>
        /// قبل از کمرک مقصد
        /// </summary>
        [Display(Name = "کمرک مقصد")]
        DestinationPort = 8,

        /// <summary>
        /// در حال حمل به کارخانه خریدار
        /// </summary>
        [Display(Name = "در حال حمل به کارخانه خریدار")]
        TransportationToCompanyLocation = 9,

        /// <summary>
        /// تحویل داده شده
        /// </summary>
        [Display(Name = "تحویل شده")] Delivered = 10,
    }
}