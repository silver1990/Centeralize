using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Bom
{
    public class EditBomProductInfoDto
    {
        public long BomProductId { get; set; }
        public decimal Quantity { get; set; }
        public AreaReadDTO Area { get; set; }
        public UserAuditLogDto UserAudit { get; set; }
        public bool IsRequiredMrp { get; set; }
        public decimal Remained { get; set; }
        public string BomReference { get; set; }

    }

}
