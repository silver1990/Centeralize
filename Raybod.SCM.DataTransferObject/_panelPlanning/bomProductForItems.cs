using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.MrpItem
{
    public class BomProductForItems : MrpItemInfoDto
    {
        public List<MrpItemInfoDto> BomChilds { get; set; }

        public BomProductForItems()
        {
            BomChilds = new List<MrpItemInfoDto>();
        }
    }
}
