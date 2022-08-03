using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //QualityControl
    public enum QCResult
    {
        /// <summary>
        /// عدم پذیرش
        /// </summary>
        [Display(Name = "عدم پذیرش")] Reject = 0,

        /// <summary>
        /// پذیرش
        /// </summary>
        [Display(Name = "پذیرش")] Accept = 1,

        [Display(Name = "ConditionalAcceptance ")] ConditionalAcceptance = 2,

    }
}