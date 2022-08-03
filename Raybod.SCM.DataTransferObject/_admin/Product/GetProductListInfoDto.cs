using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Product
{
    public class GetProductListInfoDto
    {
        public string ProductCode { get; set; }
        public string Description { get; set; }
        public string TechnicalNumber { get; set; }
        public string Unit { get; set; }
        public string ProductGroupTitle { get; set; }
        public int? ProductId { get; set; }
        public bool IsSet { get; set; }
        public bool IsRegisterd { get; set; }
        public bool IsEquipment { get; set; }
        public decimal Inventory { get; set; }

    }
}
