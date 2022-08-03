namespace Raybod.SCM.DataTransferObject.RFP
{
    public class WaitingRFPItemForAddPrContractDto : RFPItemInfoDto
    {
        public long ProductGroupId { get; set; }

        public string ProductGroupTitle { get; set; }

        public long RFPId { get; set; }

        public string RFPNumber { get; set; }

        public long? DateWinner { get; set; }
    }
}
