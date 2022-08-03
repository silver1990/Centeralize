using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.TeamWork.authentication
{
    public class UserContractEntitiesDto
    {
        public string ContractCode { get; set; }

        public List<NotifEvent> NotifEvents { get; set; }
    }
}
