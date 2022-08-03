using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Bom
{
    public class CreateBomFormAnotherBomDto
    {
        
            public decimal Quantity { get; set; }
            public AreaReadDTO Area { get; set; }


        
    }
}
