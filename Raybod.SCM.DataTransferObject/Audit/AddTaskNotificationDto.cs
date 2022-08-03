using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Audit
{
    public class AddTaskNotificationDto : AddAuditLogDto
    {
        public List<int> Users { get; set; }
    }
}
