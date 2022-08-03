using Raybod.SCM.DataTransferObject.MasterMrpReport;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Bom
{
    public class BomProductEditResultDto
    {
        public EditBomProductInfoDto BomProduct { get; set; }
        public MasterMrProductListDto MasterMr { get; set; }
    }
}
