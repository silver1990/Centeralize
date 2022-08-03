using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Services.Core.Common.Message
{
    /// <summary>
    /// نوع پیام ارسالی، از نوع خطا و یا ....
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// موفق
        /// </summary>
        [Display(Name = "success")]
        Succeed,

        /// <summary>
        /// اطلاعات
        /// </summary>
        [Display(Name = "blue")]
        Info,

        /// <summary>
        /// هشدار
        /// </summary>
        [Display(Name = "yellow")]
        Warning,

        /// <summary>
        /// خطا
        /// </summary>
        [Display(Name = "red")]
        Error,
    }
}
