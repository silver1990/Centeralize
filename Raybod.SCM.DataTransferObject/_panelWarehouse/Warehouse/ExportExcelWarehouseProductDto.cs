using System.ComponentModel;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class ExportExcelWarehouseProductDto
    {

        public string EquipmentName { get; set; }

        public string EquipmentCode { get; set; }

        public string TechnicalNumber { get; set; }

        public string Group { get; set; }

        public string Unit { get; set; }

        public decimal Inventory { get; set; }

        public string LastUpdated { get; set; }
    }
}
