using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Notification
{
    public class NotifeRecipientUserDto
    {
        public NotifEvent NotifEvent { get; set; }

        public List<UserNotifConfigDto> UserNotifConfigs { get; set; }

        public NotifeRecipientUserDto()
        {
            UserNotifConfigs = new List<UserNotifConfigDto>();
        }
    }
}
