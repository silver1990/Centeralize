using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Logistic
{
    public class PackLogisticListDto
    {
        public long PackId { get; set; }

        public string PackCode { get; set; }

        public PackStatus PackStatus { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<BaseLogisticDto> Logistics { get; set; }
        public PackLogisticListDto()
        {
            UserAudit = new UserAuditLogDto();
            Logistics = new List<BaseLogisticDto>();
        }
    }
}
