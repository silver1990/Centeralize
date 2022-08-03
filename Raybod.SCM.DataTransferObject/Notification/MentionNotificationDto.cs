using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Notification
{
    public class MentionNotificationDto
    {
        public int UnDoneCount { get; set; }

        public List<BaseMentionNotificationDto> Notifications { get; set; }

        public MentionNotificationDto()
        {
            Notifications = new List<BaseMentionNotificationDto>();
        }
    }
}
