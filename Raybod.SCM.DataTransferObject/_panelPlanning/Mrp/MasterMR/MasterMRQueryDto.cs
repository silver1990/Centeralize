using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class MasterMRQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "Id";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;

        public List<int> ProductGroupIds { get; set; }
        public List<int> AreaIds { get; set; }

        public MrpItemStatus Status { get; set; } = (MrpItemStatus)10;

    }
}
