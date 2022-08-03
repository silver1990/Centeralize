using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Role
{
    public class ListRoleDto
    {
        public string SubModuleName { get; set; }

        public List<RoleInfoDto> Roles { get; set; }
    }
}
