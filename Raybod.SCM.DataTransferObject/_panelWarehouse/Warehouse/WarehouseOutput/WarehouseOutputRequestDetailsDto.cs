using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class WarehouseOutputRequestDetailsDto
    {
        public long RequestId { get; set; }

        public string ProductGroupTitle { get; set; }

        public string RequestCode { get; set; }
        public string Note { get; set; }

        public WarehouseOutputStatus Status { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<WarehouseOutputRequestSubjectListDto> Subjects { get; set; }
        public List<WarehouseRequestConfirmationUserWorkFlowDto> WarehouseRequestConfirmationUserWorkFlows { get; set; }

        public WarehouseOutputRequestDetailsDto()
        {
            UserAudit = new UserAuditLogDto();
            WarehouseRequestConfirmationUserWorkFlows = new List<WarehouseRequestConfirmationUserWorkFlowDto>();
            Subjects = new List<WarehouseOutputRequestSubjectListDto>();
        }
    }
}
