using Raybod.SCM.DataTransferObject.TeamWork;
using System.Collections.Generic;

namespace Raybod.SCM.Services.Core.Common
{
    public class PermissionServiceResult
    {
        public bool HasPermisson { get; set; } = false;

        public bool HasOrganizationPermission { get; set; } = false;

        public List<string> ContractCodes { get; set; } = null;

        public List<TeamWorkProductsDto> ProductIds { get; set; } = null;
        public List<int> GlobalProductIds { get; set; } = null;
    }
}
