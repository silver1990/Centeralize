using Raybod.SCM.Domain.Helper;

namespace Raybod.SCM.DataTransferObject.Payment
{
    public class PaymentQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "PaymentId";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;

        public long POId { get; set; }
    }
}
