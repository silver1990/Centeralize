namespace Raybod.SCM.DataTransferObject.MrpItem
{
    public class MrpItemInfoForAddDto : BaseMrpItemDto
    {
        public string ProductCode { get; set; }

        public string ProductDescription { get; set; }

        public string ProductTechnicalNumber { get; set; }

        public string ProductGroupName { get; set; }

        public string Unit { get; set; }

        public decimal FreeQuantityInPO { get; set; }
    }
}
