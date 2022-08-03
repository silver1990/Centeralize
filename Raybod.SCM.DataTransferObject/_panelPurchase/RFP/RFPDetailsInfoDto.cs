using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class RFPDetailsInfoDto : RFPInfoDto
    {
        public List<RFPSupplierInfoDto> RFPSuppliers { get; set; }
        public string BaseContractCode { get; set; }
    }
}
