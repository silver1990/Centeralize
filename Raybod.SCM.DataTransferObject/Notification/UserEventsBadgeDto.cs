using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Notification
{
    public class UserEventsBadgeDto
    {
        public long UnSeenEventCount { get; set; }
        public long UnSeenNotificationCount { get; set; }
        public long UnSeenMentionCount { get; set; }
    }
}
