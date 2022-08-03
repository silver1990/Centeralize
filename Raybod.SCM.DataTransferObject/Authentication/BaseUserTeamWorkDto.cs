using System;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Authentication
{
    public class BaseUserTeamWorkDto
    {
        public int TeamWorkId { get; set; }

        public bool IsLatest { get; set; }

        public string TeamWorkCode { get; set; }

        public string Title { get; set; }

        public List<UserPermissionDto> UserPermissions { get; set; }
        public List<string> Services { get; set; }
        public DateTime? LastVisited { get; set; }
        public BaseUserTeamWorkDto()
        {
            UserPermissions = new List<UserPermissionDto>();
            Services = new List<string>();
        }
    }
}
