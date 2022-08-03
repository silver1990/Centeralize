using Raybod.SCM.DataTransferObject.Customer;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.TeamWork
{
    public class TeamWorkDetailsDto
    {
        public BaseTeamWorkDto TeamWork { get; set; }
      
        public List<TeamWorkUserPermissionsDto> Users { get; set; }
        public List<TeamWorkUserPermissionsDto> CustomerUsers { get; set; }
    }
}
