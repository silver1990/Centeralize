using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Notification
{
    public class UserNotifyEditDto
    {
        public long Id { get; set; }
        public int NotifyNumber { get; set; }
        public bool IsActive { get; set; }
        public NotifyManagementType NotifyType { get; set; }
    }
    
}
