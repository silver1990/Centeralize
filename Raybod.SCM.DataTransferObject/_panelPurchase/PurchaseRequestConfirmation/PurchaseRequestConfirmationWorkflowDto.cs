using Raybod.SCM.DataTransferObject.PurchaseRequest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject._panelPurchase.PurchaseRequestConfirmation
{
    public class PurchaseRequestConfirmationWorkflowDto
    {
        public string ConfirmNote { get; set; }


        public UserAuditLogDto PurchaseRequestConfirmUserAudit { get; set; }

        public List<BasePRAttachmentDto> Attachments { get; set; }
        public List<PurchaseRequestItemInfoDto> PurchaseRequestItems { get; set; }

        public List<PurchaseRequestConfirmationUserWorkFlowDto> PurchaseRequestConfirmationUserWorkFlows { get; set; }

        public PurchaseRequestConfirmationWorkflowDto()
        {
            PurchaseRequestConfirmUserAudit = new UserAuditLogDto();
            Attachments = new List<BasePRAttachmentDto>();
            PurchaseRequestConfirmationUserWorkFlows = new List<PurchaseRequestConfirmationUserWorkFlowDto>();
            PurchaseRequestItems = new List<PurchaseRequestItemInfoDto>();
        }
    }
}
