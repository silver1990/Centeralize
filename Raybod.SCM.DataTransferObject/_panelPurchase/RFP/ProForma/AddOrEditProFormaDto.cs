using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class AddOrEditProFormaDto
    {
        public List<RFPSupplierInfoDto> Suppliers { get; set; }
        public RFPProFormDetailDto ProForma { get; set; }
        public AddOrEditProFormaDto()
        {
            Suppliers = new List<RFPSupplierInfoDto>();
            
        }
    }
}
