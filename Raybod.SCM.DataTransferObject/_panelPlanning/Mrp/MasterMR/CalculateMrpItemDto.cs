using Raybod.SCM.DataTransferObject.Bom;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class CalculateMrpItemDto
    {
        public int BomProductId { get; set; }

        public List<BomForMrpDto> Boms { get; set; }
    }
}
