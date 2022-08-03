using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class PrContractConfirmationWorkflowDto
    {
        public long WorkFlowId { get; set; }

        public long PrContractConfirmWorkFlowId { get; set; } = 0;
        public string ConfirmNote { get; set; }
        public UserAuditLogDto PrContractConfirmUserAudit { get; set; }

        public List<BasePRContractConfirmationAttachmentDto> Attachments { get; set; }

        public List<PrContractConfirmationUserWorkFlowDto> PrContractConfirmationUserWorkFlows { get; set; }

        public PrContractConfirmationWorkflowDto()
        {
            PrContractConfirmUserAudit = new UserAuditLogDto();
            Attachments = new List<BasePRContractConfirmationAttachmentDto>();
            PrContractConfirmationUserWorkFlows = new List<PrContractConfirmationUserWorkFlowDto>();
        }
    }
}
