using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Payment
{
    public class PaymentListDto : BasePaymentDto
    {
        public long PaymentId { get; set; }
        public long? DateCreate { get; set; }

        public string SupplierName { get; set; }
        public PaymentStatus Status { get; set; }
        //public string SupplierCode { get; set; }
    }
    public class PendingForConfirmPaymentListDto : BasePaymentDto
    {
        public long PaymentId { get; set; }
        public long? DateCreate { get; set; }

        public string SupplierName { get; set; }
        public UserAuditLogDto UserAudit { get; set; }
        public UserAuditLogDto BallInCourtUser { get; set; }

    }
}
