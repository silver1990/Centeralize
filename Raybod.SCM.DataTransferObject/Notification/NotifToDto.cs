using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Notification
{
    public class NotifToDto
    {
        public NotifEvent NotifEvent { get; set; }
        public List<string> Roles { get; set; }
    }
}
