using Raybod.SCM.DataTransferObject.PendingForPayment;
using Raybod.SCM.DataTransferObject.PurchaseRequest;
using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Payment
{
    public class PaymentConfirmationWorkflowDto
    {
        public string ConfirmNote { get; set; }
        public string PaymentNumber { get; set; }

        public string Note { get; set; }

        public CurrencyType CurrencyType { get; set; }

        public int SupplierId { get; set; }

        public decimal Amount { get; set; }

        public string SupplierName { get; set; }
        public string SupplierLogo { get; set; }
        public UserAuditLogDto PaymentConfirmUserAudit { get; set; }

        public List<PendingForPaymentForPayedDto> PaymentItems { get; set; }

        public List<PaymentConfirmationUserWorkFlowDto> PaymentConfirmationUserWorkFlows { get; set; }

        public PaymentConfirmationWorkflowDto()
        {
            PaymentConfirmUserAudit = new UserAuditLogDto();
            PaymentConfirmationUserWorkFlows = new List<PaymentConfirmationUserWorkFlowDto>();
            PaymentItems = new List<PendingForPaymentForPayedDto>();
        }
    }
}
