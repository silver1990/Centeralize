using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Bom
{
    public class BomWithChildInfo : BomInfoDto
    {
        public int ProductGroupId { get; set; }
        public List<ListBomInfoDto> ChildBoms { get; set; }
        public AreaReadDTO Area { get; set; }
        public string ProductGroupTitle { get; set; }
    }
}
