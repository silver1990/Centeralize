using System;

namespace Raybod.SCM.DataTransferObject
{
    public class UserAuditLogDto
    {
        /// <summary>
        /// شناسه کاربر ثبت کننده
        /// </summary>
        public int? AdderUserId { get; set; }

        /// <summary>
        /// نام کاربر ثبت کننده
        /// </summary>
        public string AdderUserName { get; set; }

        /// <summary>
        /// عکس کاربر ثبت کننده
        /// </summary>
        public string AdderUserImage { get; set; }

        /// <summary>
        /// تاریخ ثبت
        /// </summary>
        public long? CreateDate { get; set; }

    }
}
