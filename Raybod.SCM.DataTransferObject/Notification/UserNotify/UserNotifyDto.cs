using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Notification
{
    public class UserNotifyWithSubDto
    {
        public long Id { get; set; }
        public int NotifyNumber { get; set; }
        public bool IsActive { get; set; }
        public string SubModuleName { get; set; }
        public NotifyManagementType NotifyType { get; set; }
    }
    public class UserNotifyDto
    {
        public long Id { get; set; }
        public int NotifyNumber { get; set; }
        public bool IsActive { get; set; }
        public NotifyManagementType NotifyType { get; set; }
        public string Description { get; set; } = "";
    }

    public class UserNotifyResultListDto
    {
        public string Module { get; set; }
        public List<UserNotifyDto> UserNotifies { get; set; }
        public UserNotifyResultListDto()
        {
            UserNotifies = new List<UserNotifyDto>();
        }
    }
}
