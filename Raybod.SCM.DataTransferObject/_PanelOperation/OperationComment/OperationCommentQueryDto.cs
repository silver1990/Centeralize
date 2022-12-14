using Raybod.SCM.Domain.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.OperationComment
{
    public class OperationCommentQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; }

        public bool IsSortAscending { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10000;

    }
}
