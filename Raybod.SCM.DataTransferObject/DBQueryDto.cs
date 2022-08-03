using Raybod.SCM.Domain.Helper;

namespace Raybod.SCM.DataTransferObject
{
    public class DBQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; }

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;
    }
}
