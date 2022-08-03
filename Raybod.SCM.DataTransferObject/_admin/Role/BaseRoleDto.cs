using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.Role
{
    public class BaseRoleDto
    { 
        /// <summary>
        /// شناسه
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// نام رول به انگلیسی - برای استفاده در سیستم
        /// </summary> 
        [Column(TypeName = "varchar")] 
        [Display(Name = "نام نقش به انگلیسی")] 
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; }

        /// <summary>
        /// نام رول - برای نمایش به کاربر
        /// </summary> 
        [StringLength(50, MinimumLength = 2)]
        [Required( ErrorMessage = "لطفا {0} را وارد کنید ")]
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
        /// ماژول
        /// </summary>
        public SCMModule SCMModule { get; set; }

        /// <summary>
        /// زیر ماژول
        /// </summary>
        [MaxLength(250)]
        [Required]
        public string SubModuleName { get; set; }

        /// <summary>
        /// سطح دسرسی ماژول
        /// </summary>
        [MaxLength(800)]
        public string ModulePermission { get; set; }

    }
}
