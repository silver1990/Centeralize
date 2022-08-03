namespace Raybod.SCM.Domain.Enum
{
    //PendingOFPeyment
    public enum PendingOFPeymentStatus
    {
        None = 0,
        /// <summary>
        /// سررسید نشده
        /// </summary>
        NotOverDue = 1,

        /// <summary>
        /// سررسید شده
        /// </summary>
        OverDue = 2,

        /// <summary>
        /// تسویه شده
        /// </summary>
        Settled = 3,
        /// <summary>
        /// لغو شده
        /// </summary>
        Canceled = 4,
    }
}
