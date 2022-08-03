using Raybod.SCM.Domain.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain
{
    public abstract class BaseAuditEntity
    {
        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// تاریخ بروزرسانی
        /// </summary>
        [Display(Name = "تاریخ بروزرسانی")]
        public DateTime? UpdateDate { get; set; }

        /// <summary>
        /// ایجاد شده توسط
        /// </summary>
        public int? AdderUserId { get; set; }

        /// <summary>
        /// بروزرسانی شده توسط
        /// </summary>
        public int? ModifierUserId { get; set; }

        [ForeignKey(nameof(AdderUserId))]
        public User AdderUser { get; set; }

        [ForeignKey(nameof(ModifierUserId))]
        public User ModifierUser { get; set; }

    }

    public abstract class BaseEntity : BaseAuditEntity
    {

        /// <summary>
        /// بررسی وضعیت حذف
        /// </summary>
        public bool IsDeleted { get; set; }

    }
}
