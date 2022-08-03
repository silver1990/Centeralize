using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Raybod.SCM.DataTransferObject.Role.Permission;

namespace Raybod.SCM.DataTransferObject.Role
{
    public class EditRoleDto : BaseRoleDto
    {
        //[Required]
        //public new int Id { get; set; }

        /// <summary>
        /// لیست پرمیشن های رول
        /// </summary>
        public List<EditPermissionDto> Permissions { get; set; }
    }
    
}
