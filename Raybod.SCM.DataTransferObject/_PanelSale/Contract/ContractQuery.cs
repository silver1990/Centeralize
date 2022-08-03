using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class ContractQuery : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "CreatedDate";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;

        public List<ContractStatus> ContractStatus { get; set; } = null;

        public List<int> CustomerIds { get; set; } = null;

        public ContractType ContractType { get; set; } = 0;
    }
}
