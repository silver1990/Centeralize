using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class ContractDescriptionDto
    {
        public string ContractCode { get; set; }
        public string Description { get; set; }
        public List<string> Services { get; set; }
        public UserAuditLogDto UserAudit { get; set; }
        public ContractDescriptionDto()
        {
            Services = new List<string>();
        }
    }
}
