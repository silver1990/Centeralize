﻿using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;

namespace Raybod.SCM.DataTransferObject.RFP.RFPComment
{
    public class RFPCommentQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; }

        public bool IsSortAscending { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;

        public RFPInqueryType InqueryType { get; set; }
    }
}
