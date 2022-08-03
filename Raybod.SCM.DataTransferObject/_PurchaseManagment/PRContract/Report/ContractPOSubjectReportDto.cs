namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class ContractPOSubjectReportDto
    {
        public long POId { get; set; }

        public int ProductId { get; set; }

        public string POCode { get; set; }

        public string MRPCode { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public long DateRequired { get; set; }

        public decimal Quantity { get; set; }

        public decimal ReceiptQuantity { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public ContractPOSubjectReportDto()
        {
            UserAudit = new UserAuditLogDto();
        }
    }
}
