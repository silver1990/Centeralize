using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class PendingForConfirmWarehouseOutputRequestDto
    {

        public long RequestId { get; set; }
        public string RequestCode { get; set; }
        public string RecepitCode { get; set; } = "";
        public string ProductGroupTitle { get; set; }
        public UserAuditLogDto UserAudit { get; set; }
        public List<string> Products { get; set; }
        public UserAuditLogDto BallInCourtUser { get; set; }
        public WarehouseOutputStatus Status { get; set; }
        public PendingForConfirmWarehouseOutputRequestDto()
        {
            UserAudit = new UserAuditLogDto();
            Products = new List<string>();
            BallInCourtUser = new UserAuditLogDto();
        }
    }
}
