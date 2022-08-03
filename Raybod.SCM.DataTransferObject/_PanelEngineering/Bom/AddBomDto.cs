using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Bom
{
    public class AddBomDto: BaseBomDto
    {
        public List<AddBomDto> ChildBoms { get; set; }
        public AreaReadDTO Area { get; set; }
        public AddBomDto()
        {
            ChildBoms = new List<AddBomDto>();
        }
    }
}
