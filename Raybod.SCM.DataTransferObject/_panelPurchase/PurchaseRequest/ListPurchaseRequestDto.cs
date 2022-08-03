using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class ListPurchaseRequestDto : BasePurchaseRequestDto
    {
        public int ProductGroupId { get; set; }

        public string ProductGroupTitle { get; set; }

        /// <summary>
        /// کوچکترین تاریخ شروع
        /// </summary>
        public long DateStart { get; set; }

        /// <summary>
        /// شماره برنامه مود
        /// </summary>
        public string MrpNumber { get; set; }

        /// <summary>
        /// بزرگترین تاریخ شروع
        /// </summary>
        public long DateEnd { get; set; }

        /// <summary>
        /// کالاها
        /// </summary>
        public List<string> Products { get; set; }

        public UserAuditLogDto UserAudit { get; set; }
        public int PurchaseItemQuantity { get; set; }
        public PurchasingStream PurchasingStream { get; set; }
    }

    
}
