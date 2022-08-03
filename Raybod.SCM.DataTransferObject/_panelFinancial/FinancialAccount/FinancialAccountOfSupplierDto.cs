namespace Raybod.SCM.DataTransferObject.FinancialAccount
{
    public class FinancialAccountOfSupplierDto : BaseFinancialAccountDto
    {
        public string RefNumber { get; set; }
        public string SupplierName { get; set; }
        public decimal PurchaseRejectAmount { get; set; }
    }
}
