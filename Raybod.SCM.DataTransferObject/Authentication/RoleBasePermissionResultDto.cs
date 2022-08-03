using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject 
{ 
    public class RoleBasePermissionResultDto
    {
        public bool HasPermission { get; set; }

        public bool HasGlobalPermission { get; set; }

        public List<TeamWorkDataDto> TeamWorkData { get; set; }
        public List<int> OperationGroupList { get; set; }

        public RoleBasePermissionResultDto()
        {
            TeamWorkData = new List<TeamWorkDataDto>();
        }
    }
}
