using Raybod.SCM.Domain.Helper;

namespace Raybod.SCM.DataTransferObject.User
{
    public class UserQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "Id";
        
        public bool IsSortAscending { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
    }
}
