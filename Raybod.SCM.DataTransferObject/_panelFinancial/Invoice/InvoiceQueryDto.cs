using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;

namespace Raybod.SCM.DataTransferObject.Invoice
{
    public class InvoiceQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "Id";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;

        public InvoiceType InvoiceType { get; set; }

        public PContractType PContractType { get; set; }

        public long PoId { get; set; }
    }
}
