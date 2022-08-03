using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.FinancialAccount
{
    public class BaseFinancialAccountDto
    {
        public long Id { get; set; }

        public int SupplierId { get; set; }

        public long? POId { get; set; }

        public long? InvoiceId { get; set; }

        public long? PaymentId { get; set; }

        public FinancialAccountType FinancialAccountType { get; set; }

        public decimal PurchaseAmount { get; set; }

        public decimal PaymentAmount { get; set; }

        public decimal RemainedAmount { get; set; }

        public long DateDone { get; set; }
        
    }
}
