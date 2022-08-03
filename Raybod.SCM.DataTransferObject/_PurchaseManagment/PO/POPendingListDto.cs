using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class POPendingListDto
    {
        /// <summary>
        /// شناسه po
        /// </summary>
        public long POId { get; set; }

        /// <summary>
        /// تاریخ تحویل
        /// </summary>
        public long DateDelivery { get; set; }

        /// <summary>
        ///  کد قرارداد خرید
        /// </summary>
        public string PRContractCode { get; set; }

        public string MrpCode { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierName { get; set; }

        public string SupplierLogo { get; set; }

        public long PRContractDateIssued { get; set; }

        public long PRContractDateEnd { get; set; }

        /// <summary>
        /// اطلاعات ثبت کننده
        /// </summary>
        public UserAuditLogDto UserAudit { get; set; }

        /// <summary>
        /// کالا ها
        /// </summary>
        public List<string> Products { get; set; }

        public POPendingListDto()
        {
            UserAudit = new UserAuditLogDto();
            Products = new List<string>();
        }
    }
}
