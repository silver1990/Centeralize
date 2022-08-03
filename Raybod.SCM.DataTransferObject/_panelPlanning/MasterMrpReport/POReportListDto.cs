namespace Raybod.SCM.DataTransferObject.MasterMrpReport
{
    public class POReportListDto
    {
        public long POSubjectId { get; set; }

        public long POId { get; set; }

        public string POCode { get; set; }

        public long? CreateDate { get; set; }

        public long DateDelivery { get; set; }

        public string PRContractCode { get; set; }

        public string MrpNumber { get; set; }

        public decimal Quantity { get; set; }

        public decimal ReceiptedQuantity { get; set; }

        public decimal RemainedQuantity { get; set; }

    }
}
