using Raybod.SCM.Domain.Extention;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class User
    {
        public int Id { get; set; }
        /// <summary>
        /// نام
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی
        /// </summary>
        [Required]
        [StringLength(100)]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [MaxLength(150)]
        public string FullName
        {
            get;
            set;
        }

        ///// <summary>
        ///// کد پرسنلی
        ///// </summary>     
        //[Display(Name = "کد پرسنلی")]
        //[MaxLength(50)]
        //public string PersonalCode { get; set; }

        /// <summary>
        /// شماره موبایل
        /// </summary>
        [Required]
        [Display(Name = "شماره موبایل")]
        [RegularExpression(@"0\d{10}$")]
        public string Mobile { get; set; }

        /// <summary>
        /// نام کاربری
        /// </summary>

        [StringLength(50, MinimumLength = 5)]
        public string UserName { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        [EmailAddress]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        /// <summary>
        /// تلفن ثابت
        /// </summary>
        [Display(Name = "تلفن ثابت")]
        public string Telephone { get; set; }

        /// <summary>
        /// بررسی وضعیت حذف
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// وضعیت کاربر
        /// </summary>
        [Display(Name = "وضعیت کاربر")]
        public bool IsActive { get; set; } = true;


        /// <summary>
        /// تصویر 
        /// </summary>
        [MaxLength(300)]
        public string Image { get; set; }

        ///// <summary>
        ///// امضاء 
        ///// </summary>
        //[MaxLength(300)]
        //public string Signature { get; set; }

        public DateTime? DateExpireRefreshToken { get; set; }

        [MaxLength(50)]
        public string RefreshToken { get; set; }

        /// <summary>
        /// گذرواژه
        /// </summary>
        private string _password;

        [Column(TypeName = "varchar(64)")]
        [MaxLength(64)]
        public string Password
        {
            get
            {
                return _password.TryTrim();
            }
            set
            {
                _password = value.TryTrim();
            }
        }

        /// <summary>
        /// نوع کاربر
        /// </summary>
        public int UserType { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual ICollection<TeamWorkUser> TeamWorkUsers { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Notification> PerformerNotifications { get; set; }
        public virtual ICollection<UserLatestTeamWork> UserLatestTeamWorks { get; set; }
    }
}
