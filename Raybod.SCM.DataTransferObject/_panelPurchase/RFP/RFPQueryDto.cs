using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class RFPQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "Id";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;

        public PRDateQuery PRDateQuery { get; set; }

        public long? FromDateTime { get; set; }

        public long? ToDateTime { get; set; }

        public List<RFPStatus> Statuses { get; set; }

        public List<string> ContractCodes { get; set; }

        public List<int> ProductGroups { get; set; }

        public List<int> Products { get; set; }

        public List<int> Suppliers { get; set; }
        public string RfpNumber { get; set; }
    }
}
