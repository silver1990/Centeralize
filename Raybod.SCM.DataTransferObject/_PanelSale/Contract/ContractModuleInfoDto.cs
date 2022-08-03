using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelSale.Contract
{
    public class ContractModuleInfoDto
    {
        public string ContractCode { get; set; }
        public bool DocumentManagement { get; set; }
        public bool PurchaseManagement { get; set; }
        public bool ConstractionManagement { get; set; }
        public bool FileDrive { get; set; }
    }
}
