using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Product
{
    public class ProductQuery : IQueryObject
    {
        public string SearchText { get; set; }

        public List<int> ProductGroupIds { get; set; }

        public string SortBy { get; set; } = "ProductId";

        public bool IsSortAscending { get; set; } = false;

        public ProductType ProductType { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 1500;

    }
}
