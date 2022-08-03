using Raybod.SCM.DataTransferObject.ProductGroup.Group;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.ProductGroup
{
    public class TeamWorkProductGroupDto
    {        
        public List<BaseProductGroupDto> ProductGroups { get; set; }

        public TeamWorkProductGroupDto()
        {            
            ProductGroups = new List<BaseProductGroupDto>();
        }
    }
}
