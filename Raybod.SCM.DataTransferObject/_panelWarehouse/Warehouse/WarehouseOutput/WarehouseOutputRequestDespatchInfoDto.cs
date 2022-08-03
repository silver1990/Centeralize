using Raybod.SCM.DataTransferObject.Warehouse;
using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class WarehouseOutputRequestDespatchInfoDto
    {
        
            public long RequestId { get; set; }

            public string ProductGroupTitle { get; set; }

            public string RequestCode { get; set; }
            public long? RecepitId { get; set; }
            public string RecepitCode { get; set; }

            public WarehouseOutputStatus Status { get; set; }

            public UserAuditLogDto UserAudit { get; set; }

            public List<WarehouseOutputRequestSubjectListDto> Subjects { get; set; }
           
            public WarehouseOutputRequestDespatchInfoDto()
            {
                UserAudit = new UserAuditLogDto();
                Subjects = new List<WarehouseOutputRequestSubjectListDto>();
            }
        
    }
}
