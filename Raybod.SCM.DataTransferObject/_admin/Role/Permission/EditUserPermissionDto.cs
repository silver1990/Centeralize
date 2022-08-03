using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Role.Permission
{
    public class EditUserPermissionDto
    {
        [Required]
        public new int Id { get; set; }

        /// <summary>
        /// لیست پرمیشن های رول
        /// </summary>
        public List<EditPermissionDto> Permissions { get; set; }
    }
}