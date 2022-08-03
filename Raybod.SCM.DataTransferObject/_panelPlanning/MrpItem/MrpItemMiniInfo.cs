namespace Raybod.SCM.DataTransferObject.MrpItem
{
    public class MrpItemMiniInfo
    {
        public long Id { get; set; }

        public int ProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductDescription { get; set; }

        public long MrpId { get; set; }

        public string Unit { get; set; }

        public decimal FinalRequirment { get; set; }
    }
}
