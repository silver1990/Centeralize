using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Role
{
    public class RoleInfoDto
    {
        public int RoleId { get; set; }


        /// <summary>
        /// نام رول - برای نمایش به کاربر
        /// </summary> 
        [StringLength(50, MinimumLength = 2)]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید ")]
        [Display(Name = "نام نقش به فارسی")]
        public string DisplayName { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(500)]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        /// امکان تعریف به صورت موازی
        /// </summary>
        public bool IsParalel { get; set; } = false;

        /// <summary>
        /// آی دی متناظر با شماره work flow در نوع شمارشی خودش 
        /// </summary>
        public int? WorkFlowStateId { get; set; }

        /// <summary>
        /// work flow section
        /// </summary>
        public SCMWorkFlow SCMWorkFlow { get; set; }


        /// <summary>
        /// سطح دسرسی ماژول
        /// </summary>
        [MaxLength(800)]
        public string ModulePermission { get; set; }
    }
}
