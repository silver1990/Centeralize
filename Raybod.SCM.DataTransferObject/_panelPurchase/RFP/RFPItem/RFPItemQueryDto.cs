using Raybod.SCM.Domain.Helper;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class RFPItemQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "Id";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;

        public int ProductGroupId { get; set; }

        public List<int> Products { get; set; }

        public int SupplierId { get; set; }
    }
}
