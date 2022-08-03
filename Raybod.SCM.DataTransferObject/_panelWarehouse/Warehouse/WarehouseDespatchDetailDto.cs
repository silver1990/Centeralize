using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class WarehouseDespatchDetailDto
    {
        public long RequestId { get; set; }
        public long DespatchId { get; set; }

        public string ProductGroupTitle { get; set; }

        public string RequestCode { get; set; }
        public string DespatchCode { get; set; }

        public WarehouseDespatchStatus Status { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<WarehouseOutputRequestSubjectListDto> Subjects { get; set; }
      
        public WarehouseDespatchDetailDto()
        {
            UserAudit = new UserAuditLogDto();
            Subjects = new List<WarehouseOutputRequestSubjectListDto>();
        }
    }
}
