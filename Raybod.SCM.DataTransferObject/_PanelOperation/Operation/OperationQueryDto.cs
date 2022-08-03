using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Operation
{
    public class OperationQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "OperationId";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10000;


        public List<OperationStatus> OperationStatuses { get; set; }

        public List<int> OperationGroupIds { get; set; }
        public List<int> AreaIds { get; set; }


    }
}
