using System.Collections.Generic;
using Raybod.SCM.DataTransferObject.Role.Permission;

namespace Raybod.SCM.DataTransferObject.Role
{
    public class AddRoleDto : BaseRoleDto
    {
        /// <summary>
        /// لیست پرمیشن های رول
        /// </summary>
        public List<PermissionDto> Permissions { get; set; }
    }
}
