namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class ExportToExcelMrpItemDto
    {

        public string productCode { get; set; }

        public string productDescription { get; set; }

        public string productTechnicalNumber { get; set; }

        public string productGroupName { get; set; }

        public string unit { get; set; }

        public decimal freeQuantityInPO { get; set; }

        public int productId { get; set; }

        public decimal grossRequirement { get; set; }

        public decimal warehouseStock { get; set; }

        public decimal reservedStock { get; set; }

        public decimal netRequirement { get; set; }

        public decimal surplusQuantity { get; set; }

        public decimal finalRequirment { get; set; }

        public decimal po { get; set; }

        public decimal pr { get; set; }

        public long dateStart { get; set; }

        public long dateEnd { get; set; }
    }
}
