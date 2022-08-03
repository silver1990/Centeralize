using Raybod.SCM.Domain.Helper;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Transmittal
{
    public class TransmittalQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "TransmittalId";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        //public DocumentClass DocClass { get; set; }

        public List<int> DocumentGroupIds { get; set; }
        public List<long> RevisionIds { get; set; }
    }
}
