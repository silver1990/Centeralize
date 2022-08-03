using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject
{
    public class SCMFormPermisionDto
    {
        public List<SCMModule> SCMModules { get; set; }

        public List<SCMFormPermission> SCMFormPermissions { get; set; }

        public SCMFormPermisionDto()
        {
            SCMModules = new List<SCMModule>();
            SCMFormPermissions = new List<SCMFormPermission>();
        }
    }
}
