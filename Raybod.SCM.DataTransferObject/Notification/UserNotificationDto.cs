using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Notification
{
    public class UserNotificationDto
    {
        public int UnDoneCount { get; set; }


        public List<BaseNotificationDto> Notifications { get; set; }

        public UserNotificationDto()
        {
            Notifications = new List<BaseNotificationDto>();
        }
    }
}
