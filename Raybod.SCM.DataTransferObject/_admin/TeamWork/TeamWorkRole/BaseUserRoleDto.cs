using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.TeamWork
{
    public class BaseUserRoleDto
    {
        public int RoleId { get; set; }

        /// <summary>
        /// آیا نوتیف ارسال شود
        /// </summary>
        public bool IsSendNotification { get; set; }

        /// <summary>
        /// آیا نوتیف به صورت ایمیل ارسال شود
        /// </summary>
        public bool IsSendMailNotification { get; set; }

    }
}
