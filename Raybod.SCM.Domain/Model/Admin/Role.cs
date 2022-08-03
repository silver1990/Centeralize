using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Model
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// نام رول به انگلیسی - برای استفاده در سیستم
        /// </summary>
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; }

        /// <summary>
        /// نام رول - برای نمایش به کاربر
        /// </summary>
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string DisplayName { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// امکان تعریف به صورت موازی
        /// </summary>
        public bool IsGlobalGroup { get; set; } = false;
        
        /// <summary>
        /// زیر ماژول
        /// </summary>
        [MaxLength(250)]
        [Required]
        public string SubModuleName { get; set; }

        /// <summary>
        /// رویداد
        /// </summary>
        public string SCMEvents { get; set; }
        /// <summary>
        /// تسک های دریافتی
        /// </summary>
        public string SCMTasks { get; set; }
        /// <summary>
        ///ایمیل های دریافتی
        /// </summary>
        public string SCMEmails { get; set; }

        /// <summary>
        /// آیا نوتیف ارسال شود
        /// </summary>
        public bool IsSendNotification { get; set; } = true;

        /// <summary>
        /// آیا نوتیف به صورت ایمیل ارسال شود
        /// </summary>
        public bool IsSendMailNotification { get; set; } = true;

        /// <summary>
        /// یوزرهایی که این رول را دارند
        /// </summary>
        //public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<TeamWorkUserRole> TeamWorkUserRoles { get; set; }



    }
}
