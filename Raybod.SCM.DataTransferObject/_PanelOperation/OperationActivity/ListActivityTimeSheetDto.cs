using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelOperation
{
    public class  ListActivityTimeSheetDto:BaseActivityTimeSheetDto
    {
        public UserAuditLogDto UserAudit { get; set; }
        public string TotalDuration { get; set; }
        public ListActivityTimeSheetDto()
        {
            UserAudit = new UserAuditLogDto();
        }
    }
}
