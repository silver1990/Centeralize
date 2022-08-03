using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;

namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class MrpQuery : IQueryObject
    {
        public string SearchText { get; set; }
        public string MrpNumber { get; set; }

        public string SortBy { get; set; } = "CreatedDate";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;

    }
}
