using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class MrpInfoDto : EditMrpDto
    {
        public List<string> Products { get; set; }
        public int MrpItemQuantity { get; set; }

        public string ProductGroupTitle { get; set; }

        public int ProductGroupId { get; set; }

        public UserAuditLogDto UserAudit { get; set; }
        public PurchasingStream PurchasingStream { get; set; }
    }
}
