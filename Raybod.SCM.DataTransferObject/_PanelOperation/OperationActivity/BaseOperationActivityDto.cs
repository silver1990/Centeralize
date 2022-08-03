using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelOperation
{
    public class BaseOperationActivityDto: AddOperationActivityDto
    {
        public long OperationActivityId { get; set; }

        public OperationActivityStatus OperationActivityStatus { get; set; }

        public string Duration { get; set; }

        public UserAuditLogDto ActivityOwner { get; set; }
        public double ProgressPercent { get; set; }

        public BaseOperationActivityDto()
        {
            ActivityOwner = new UserAuditLogDto();
        }
    }
}
