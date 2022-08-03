using Raybod.SCM.Domain.Helper;

namespace Raybod.SCM.DataTransferObject.TeamWork
{
    public class TeamWorkQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "Id";

        public bool IsSortAscending { get; set; } = true;

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
