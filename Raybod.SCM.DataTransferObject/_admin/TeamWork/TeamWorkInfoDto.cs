using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.TeamWork
{
    public class TeamWorkInfoDto : BaseTeamWorkDto
    {
        public int UserCount { get; set; }
        public List<UserAuditLogDto> users { get; set; }
    }
}
