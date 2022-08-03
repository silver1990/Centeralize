namespace Raybod.SCM.DataTransferObject.MasterMrpReport
{
    public class PRReportListDto
    {
        public long PRItemId { get; set; }

        public long PRId { get; set; }

        public string PRCode { get; set; }

        public string MRPNumber { get; set; }

        public decimal Quntity { get; set; }

        public long? CreateDate { get; set; }
    }
}
