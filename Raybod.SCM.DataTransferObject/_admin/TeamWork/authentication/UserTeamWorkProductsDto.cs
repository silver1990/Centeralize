using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.TeamWork.authentication
{
    public class UserTeamWorkProductsDto
    {
        public string ContractCode { get; set; }

        public int TeamWorkId { get; set; }

        public int UserId { get; set; }

        public List<int> ProductIds { get; set; }
    }
}
