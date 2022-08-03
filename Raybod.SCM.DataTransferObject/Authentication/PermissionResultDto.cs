using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject
{
    public class PermissionResultDto
    {
        public bool HasPermission { get; set; }

        public bool HasGlobalPermission { get; set; }

        public List<int> ProductGroupIds { get; set; }

        public List<int> DocumentGroupIds { get; set; }

        public List<TeamWorkDataDto> TeamWorkData { get; set; }

        public PermissionResultDto()
        {
            TeamWorkData = new List<TeamWorkDataDto>();
        }
    }
}
