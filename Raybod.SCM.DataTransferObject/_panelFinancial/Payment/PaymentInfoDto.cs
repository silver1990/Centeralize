using Raybod.SCM.DataTransferObject.PendingForPayment;
using Raybod.SCM.DataTransferObject.Supplier;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Payment
{
    public class PaymentInfoDto : BasePaymentDto
    {
        public long PaymentId { get; set; }

        public long? DateCreate { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<PendingForPaymentForPayedDto> PendingForPayments { get; set; }

        public string SupplierName { get; set; }
        public string SupplierLogo { get; set; }
        public string SupplierCode { get; set; }

        public List<BasePaymentAttachmentDto> PaymentAttachment { get; set; }

        public PaymentInfoDto()
        {
            PendingForPayments = new List<PendingForPaymentForPayedDto>();
            PaymentAttachment = new List<BasePaymentAttachmentDto>();
            UserAudit = new UserAuditLogDto();
        }
    }

    public class PaymentInfoWithWorkFlowDto : BasePaymentDto
    {
        public long PaymentId { get; set; }

        public long? DateCreate { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<PendingForPaymentForPayedDto> PendingForPayments { get; set; }

        public string SupplierName { get; set; }
        public string SupplierLogo { get; set; }
        public string SupplierCode { get; set; }

        public List<PaymentConfirmationUserWorkFlowDto> PaymentConfirmationUserWorkFlows { get; set; }
        public PaymentInfoWithWorkFlowDto()
        {
            PendingForPayments = new List<PendingForPaymentForPayedDto>();
            UserAudit = new UserAuditLogDto();
            PaymentConfirmationUserWorkFlows = new List<PaymentConfirmationUserWorkFlowDto>();
        }
    }
}
