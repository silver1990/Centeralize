using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.MasterMrpReport
{
    public class RFPReportListDto
    {
        public long RFPItemId { get; set; }

        public long RFPId { get; set; }

        public string RFPNumber { get; set; }

        public RFPStatus RFPStatus { get; set; }

        public string PRCode { get; set; }

        public decimal Quntity { get; set; }

        public long? CreateDate { get; set; }
    }
}
