using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Audit
{
    public class AuditLogNotificationDto
    {
        public int UnSeenCount { get; set; }

        public List<AuditLogMiniInfoDto> Notifications { get; set; }

        public AuditLogNotificationDto()
        {
            Notifications = new List<AuditLogMiniInfoDto>();
        }
    }
}
