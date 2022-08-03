using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class ListPrContractConfirmationUserWorkFlowDto
    {
        public UserAuditLogDto ConfirmUserAudit { get; set; }

        public string ConfirmNote { get; set; }

        public List<PrContractConfirmationUserWorkFlowDto> PrContractConfirmationUserWorkFlows { get; set; }
        public ListPrContractConfirmationUserWorkFlowDto()
        {
            ConfirmUserAudit = new UserAuditLogDto();
            PrContractConfirmationUserWorkFlows = new List<PrContractConfirmationUserWorkFlowDto>();
        }
    }
}
