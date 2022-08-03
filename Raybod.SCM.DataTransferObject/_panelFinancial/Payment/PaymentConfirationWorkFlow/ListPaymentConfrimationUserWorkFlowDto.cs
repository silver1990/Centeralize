using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Payment
{
    public class ListPaymentConfrimationUserWorkFlowDto
    {
        public UserAuditLogDto ConfirmUserAudit { get; set; }

        public string ConfirmNote { get; set; }

        public List<PaymentConfirmationUserWorkFlowDto> PaymentConfirmationUserWorkFlows { get; set; }
        public ListPaymentConfrimationUserWorkFlowDto()
        {
            ConfirmUserAudit = new UserAuditLogDto();
            PaymentConfirmationUserWorkFlows = new List<PaymentConfirmationUserWorkFlowDto>();
        }
    }
}
