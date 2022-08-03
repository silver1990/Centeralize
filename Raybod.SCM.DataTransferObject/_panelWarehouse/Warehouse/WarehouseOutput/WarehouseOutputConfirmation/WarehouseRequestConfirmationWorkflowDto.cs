using Raybod.SCM.DataTransferObject.PurchaseRequest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class WarehouseRequestConfirmationWorkflowDto
    {
        public string ConfirmNote { get; set; }
        public string RequestCode { get; set; }

        public UserAuditLogDto WarehouseOutputRequestConfirmUserAudit { get; set; }

        public List<WarehouseOutputRequestSubjectListDto> WarehouseOutputRequestSubjects { get; set; }

        public List<WarehouseRequestConfirmationUserWorkFlowDto> WarehouseOutputRequestConfirmationUserWorkFlows { get; set; }

        public WarehouseRequestConfirmationWorkflowDto()
        {
            WarehouseOutputRequestConfirmUserAudit = new UserAuditLogDto();
            WarehouseOutputRequestConfirmationUserWorkFlows = new List<WarehouseRequestConfirmationUserWorkFlowDto>();
            WarehouseOutputRequestSubjects = new List<WarehouseOutputRequestSubjectListDto>();
        }
    }
}
