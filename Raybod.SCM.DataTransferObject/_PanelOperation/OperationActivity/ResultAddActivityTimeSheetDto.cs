using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelOperation
{
    public class  ResultAddActivityTimeSheetDto: ActivityTimeSheetDto
    {

        public string Description { get; set; }


        public string Duration { get; set; }


        public long DateIssue { get; set; }

        public double? ProgressPercent { get; set; }
        public long ActivityTimesheetId { get; set; }
        public UserAuditLogDto UserAudit { get; set; }
        public string TotalDuration { get; set; }
        public ResultAddActivityTimeSheetDto()
        {
            UserAudit = new UserAuditLogDto();
        }

    }
}
