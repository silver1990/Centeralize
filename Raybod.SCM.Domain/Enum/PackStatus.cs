using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //Pack
    public enum PackStatus
    {
        /// <summary>
        /// ثبت شده
        /// </summary>
        Register = 1,

        /// <summary>
        /// رد QC
        /// </summary>
        RejectQC = 2,

        /// <summary>
        /// قبول QC
        /// </summary>
        AcceptQC = 3,

        /// <summary>
        /// در حال حمل به کمرک مبدا
        /// </summary>
        [Display(Name = "در حال حمل به کمرک مبدا")]
        T1Inprogress = 4,

        /// <summary>
        /// درانتظار ترخیص از کمرک مبدا
        /// </summary>
        [Display(Name = "درانتظار ترخیص از کمرک مبدا")]
        C1Pending = 5,

        /// <summary>
        /// درحال ترخیص شده از کمرک مبدا
        /// </summary>
        [Display(Name = "درحال ترخیص شده از کمرک مبدا")]
        C1Inprogress = 6,

        /// <summary>
        /// در انتظار حمل به کمرک مقصد
        /// </summary>
        [Display(Name = "در انتظار حمل به کمرک مقصد")]
        T2Pending = 7,

        /// <summary>
        /// در حال حمل به کمرک مقصد
        /// </summary>
        [Display(Name = "در حال حمل به کمرک مقصد")]
        T2Inprogress = 8,

        /// <summary>
        /// در انتظار ترخیص از کمرک مقصد
        /// </summary>
        [Display(Name = "در انتظار ترخیص از کمرک مقصد")]
        C2Pending = 9,

        /// <summary>
        /// ترخیص شده از کمرک مقصد
        /// </summary>
        [Display(Name = "در حال ترخیص از کمرک مقصد")]
        C2Inprogress = 10,

        /// <summary>
        /// در حال حمل به انبار خریدار
        /// </summary>
        [Display(Name = "در انتظار حمل به انبار خریدار")]
        T3Pending = 11,

        /// <summary>
        /// در حال حمل به کمرک مقصد
        /// </summary>
        [Display(Name = "در حال حمل به انبار خریدار")]
        T3Inprogress = 12,

        /// <summary>
        /// درانتظار تحویل
        /// </summary>
        [Display(Name = "در انتظار تحویل")]
        PendingDelivered = 13,

        /// <summary>
        /// تحویل داده شد
        /// </summary>
        [Display(Name = "تحویل داده شد")]
        Delivered = 14

    }
}
