using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.User
{
    public class BaseUserDto
    {

        /// <summary>
        /// نام
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        [StringLength(100, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        ///// <summary>
        ///// کد پرسنلی
        ///// </summary>
        //[Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        //[Display(Name = "کد پرسنلی")]
        //[StringLength(50, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        //public string PersonalCode { get; set; }

        /// <summary>
        /// شماره موبایل
        /// </summary>
        //[Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        //[Display(Name = "شماره موبایل")]
        //[RegularExpression(@"(\+98|0)?9\d{9}", ErrorMessage = "لطفا درست وارد نمایید")]
        public string Mobile { get; set; }

        /// <summary>
        /// نام کاربری
        /// </summary>
       
        [StringLength(50, MinimumLength = 5, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; }

        /// <summary>
        /// گذرواژه
        /// </summary>
       
        public string Pass { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        [EmailAddress(ErrorMessage = "آدرس ایمیل اشتباه وارد شده است")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        /// <summary>
        /// تلفن ثابت
        /// </summary>
        [Display(Name = "تلفن ثابت")]
        public string Telephone { get; set; }

        /// <summary>
        /// وضعیت کاربر
        /// </summary>
        [Display(Name = "وضعیت کاربر")]
        public bool IsActive { get; set; } = true;

        ///// <summary>
        ///// سوپر ادمین
        ///// </summary>
        //[Display(Name = "سوپر ادمین")]
        //public bool IsSuperUser { get; set; } = false;

        ///// <summary>
        ///// انتخاب بعنوان کاربر سیستم
        ///// </summary>
        //[Display(Name = "کاربر سیستم")]
        //public bool IsUser { get; set; } = true;
     
        /// <summary>
        /// تصویر 
        /// </summary>
        [MaxLength(300, ErrorMessage = "حداقل مقدار برای فیلد {1} می باشد.")]
        [Display(Name = "عکس")]
        public string Image { get; set; }

        ///// <summary>
        ///// امضاء 
        ///// </summary>

        //[Display(Name = "امضاء")]
        //[MaxLength(300, ErrorMessage = "حداقل مقدار برای فیلد {1} می باشد.")]
        //public string Signature { get; set; }
        /// <summary>
        /// نوع کاربر
        /// </summary>
        public int UserType { get; set; }
    }
}