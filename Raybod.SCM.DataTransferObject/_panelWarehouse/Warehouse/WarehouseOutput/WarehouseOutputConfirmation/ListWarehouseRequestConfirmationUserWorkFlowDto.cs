using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class ListWarehouseRequestConfirmationUserWorkFlowDto
    {
        public UserAuditLogDto ConfirmUserAudit { get; set; }

        public string ConfirmNote { get; set; }

        public List<WarehouseRequestConfirmationUserWorkFlowDto> WarehouseOutputRequestConfirmationUserWorkFlows { get; set; }
        public ListWarehouseRequestConfirmationUserWorkFlowDto()
        {
            ConfirmUserAudit = new UserAuditLogDto();
            WarehouseOutputRequestConfirmationUserWorkFlows = new List<WarehouseRequestConfirmationUserWorkFlowDto>();
        }
    }
}
