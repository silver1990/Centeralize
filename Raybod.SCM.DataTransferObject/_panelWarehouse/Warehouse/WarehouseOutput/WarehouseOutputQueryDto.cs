using Raybod.SCM.Domain.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class WarehouseOutputQueryDto:IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "RequestId";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;

        public List<int> ProductGroupIds { get; set; }


    }
}
