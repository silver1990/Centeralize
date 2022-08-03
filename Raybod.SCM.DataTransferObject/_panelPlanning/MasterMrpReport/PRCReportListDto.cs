namespace Raybod.SCM.DataTransferObject.MasterMrpReport
{
    public class PRCReportListDto
    {
        public long PRCSubjectId { get; set; }

        public long PRContractId { get; set; }

        public string PRContractCode { get; set; }

        public long? CreateDate { get; set; }

        public long DateIssued { get; set; }

        public long DateEnd { get; set; }

        public decimal Quntity { get; set; }

        public string RFPNumber { get; set; }

        public decimal OrderQuantity { get; set; }

        public decimal ReceiptQuantity { get; set; }

        public decimal RemainedQuantity{ get; set; }

    }
}
