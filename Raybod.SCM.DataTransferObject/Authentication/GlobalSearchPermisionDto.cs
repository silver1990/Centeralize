using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject
{
    public class GlobalSearchPermisionDto
    {
        public SCMFormPermission SCMForm { get; set; }

        public bool IsHaveGlobalPermision { get; set; }

        public List<string> ContractCodes { get; set; }
    }
}
