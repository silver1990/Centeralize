using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject._panelPurchase.PurchaseRequestConfirmation
{
    public class ListPurchaseRequestConfirmationUserWorkFlowDto
    {
        public UserAuditLogDto ConfirmUserAudit { get; set; }

        public string ConfirmNote { get; set; }

        public List<PurchaseRequestConfirmationUserWorkFlowDto> PurchaseRequestConfirmationUserWorkFlows { get; set; }
        public ListPurchaseRequestConfirmationUserWorkFlowDto()
        {
            ConfirmUserAudit = new UserAuditLogDto();
            PurchaseRequestConfirmationUserWorkFlows = new List<PurchaseRequestConfirmationUserWorkFlowDto>();
        }
    }
}
