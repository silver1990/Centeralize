using Raybod.SCM.DataTransferObject.PO;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.MrpItem
{
    public class AddMrpItemDto : BaseMrpItemDto
    {
        public List<AddPOByMrpDto> AddPoModel { get; set; }

        public AddMrpItemDto()
        {
            AddPoModel = new List<AddPOByMrpDto>();
        }
    }
}
