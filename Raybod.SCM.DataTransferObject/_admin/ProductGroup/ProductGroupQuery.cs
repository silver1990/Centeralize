using Raybod.SCM.Domain.Helper;

namespace Raybod.SCM.DataTransferObject.ProductGroup
{
    public class ProductGroupQuery : IQueryObject
    {
        public int? ParentId { get; set; }
        public string SearchText { get; set; }
        public string SortBy { get; set; } = "Id";
        public bool IsSortAscending { get; set; } = false;
        public int Page { get; set; }
        public int PageSize { get; set; } = 1500;
    }
}
