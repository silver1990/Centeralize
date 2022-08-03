using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class PurchaseRequestQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "Id";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;

        public PRDateQuery PRDateQuery { get; set; }

        public long? FromDateTime { get; set; }

        public long? ToDateTime { get; set; }

        public List<string> ContractCodes { get; set; }

        public List<int> ProductGroupIds { get; set; }

        public List<int> ProductIds { get; set; }

        public List<PRStatus> Status { get; set; }
        public string PurchaseNumber { get; set; }


    }
}
