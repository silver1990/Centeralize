using System.Collections.Generic;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class PRContractQuery : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "Id";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;

        public List<int> ProductGroupIds { get; set; }

        public List<int> ProductIds { get; set; }

        public List<int> SupplierIds { get; set; }

        public List<PRContractStatus> PRContractStatuses { get; set; } = null;

    }
    
}