using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Notification
{
    public class UserNotifyListDto
    {
        public List<UserNotifyResultListDto> Emails { get; set; }
        public List<UserNotifyResultListDto> Events { get; set; }
        public List<UserNotifyResultListDto> Reminders { get; set; }
        public UserNotifyListDto()
        {
            Emails = new List<UserNotifyResultListDto>();
            Events = new List<UserNotifyResultListDto>();
            Reminders = new List<UserNotifyResultListDto>();
        }
       
    }
}
