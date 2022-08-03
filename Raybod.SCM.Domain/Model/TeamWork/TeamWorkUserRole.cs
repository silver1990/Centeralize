using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.Domain.Model
{
    public class TeamWorkUserRole
    {
        [Key]
        public int Id { get; set; }

        public int RoleId { get; set; }

        public int TeamWorkUserId { get; set; }

        public int TeamWorkId { get; set; }

        public int UserId { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }

        public string RoleName { get; set; }

        public string RoleDisplayName { get; set; }

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


        [ForeignKey(nameof(TeamWorkUserId))]
        public virtual TeamWorkUser TeamWorkUser { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; }

    }
}
