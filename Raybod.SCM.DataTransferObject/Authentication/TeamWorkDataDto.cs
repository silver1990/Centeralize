using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject
{
    public class TeamWorkDataDto
    {
        public string ContractCode { get; set; }

        public List<int> ProductGroupIds { get; set; }

        public TeamWorkDataDto()
        {
            ProductGroupIds = new List<int>();
        }
    }
}
