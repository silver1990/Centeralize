using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Authentication
{
    public class UserPermissionDto
    {
        public string Permission { get; set; }

        public List<int> PermissionIds { get; set; }

        public UserPermissionDto()
        {
            PermissionIds = new List<int>();
        }
    }
}
