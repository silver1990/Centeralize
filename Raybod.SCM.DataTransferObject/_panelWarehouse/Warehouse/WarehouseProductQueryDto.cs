using Raybod.SCM.Domain.Helper;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class WarehouseProductQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "Id";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;

        public int ProductGroupId { get; set; }

        public List<int> ProductGroupIds { get; set; }

        public List<int> ProductIds { get; set; }

        //public List<int> SupplierIds { get; set; }
    }
}
