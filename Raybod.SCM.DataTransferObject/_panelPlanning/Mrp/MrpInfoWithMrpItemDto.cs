using Raybod.SCM.DataTransferObject.MrpItem;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class MrpInfoWithMrpItemDto : MrpInfoDto
    {
        public List<MrpItemInfoDto> MrpItems { get; set; }
        public MrpInfoWithMrpItemDto()
        {
            MrpItems = new List<MrpItemInfoDto>();
        }
    }
}
