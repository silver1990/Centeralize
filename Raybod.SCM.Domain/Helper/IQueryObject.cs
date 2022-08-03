namespace Raybod.SCM.Domain.Helper
{
    public interface IQueryObject
    {
        string SearchText { get; set; }
        string SortBy { get; set; }
        bool IsSortAscending { get; set; }
        int Page { get; set; }
        int PageSize { get; set; }
    }
}
