using Raybod.SCM.DataTransferObject.Audit;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Notification
{
    public class AddNotificationDto : IAuditLogObject
    {
        /// <summary>
        /// شناسه قرارداد فروش
        /// </summary>
        public string ContractCode { get; set; }

        /// <summary>
        /// نام ثبت کننده فعالیت
        /// </summary>
        public string PerformerUserFullName { get; set; }

        /// <summary>
        /// شناسه ثبت کننده فعالیت
        /// </summary>
        public int PerformerUserId { get; set; }

        /// <summary>
        /// رویداد
        /// </summary>
        public NotifEvent NotifEvent { get; set; }

        /// <summary>
        ///  شماره فرم
        /// </summary>
        public string FormCode { get; set; }

        /// <summary>
        /// شرح
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///  تعداد
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        /// شناسه فرم (tbl)
        /// </summary>
        public string KeyValue { get; set; }

        public string RootKeyValue { get; set; }
    
    }
}
