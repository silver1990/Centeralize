using Raybod.SCM.DataTransferObject.MrpItem;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class AddMrpDto
    {
        public List<AddMrpItemDto> MrpItems { get; set; }
    }
}
