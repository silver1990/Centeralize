using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.TeamWork.authentication
{
    public class UserTeamWorkRoleDto
    {
        public int TeamWorkId { get; set; }

        public bool HasOrganizationPermision { get; set; }

        public List<string> ContractCodes { get; set; }

        public int UserId { get; set; }

        public int? WorkFlowStateId { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public bool IsParalel { get; set; }

        /// <summary>
        /// work flow section
        /// </summary>
        public SCMWorkFlow SCMWorkFlow { get; set; }

    }
}
