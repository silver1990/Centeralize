using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Packing
{
    public class PackQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "PackId";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;

        public long? poId { get; set; } 

        public List<PackStatus> PackStatuses { get; set; }

    }
}
