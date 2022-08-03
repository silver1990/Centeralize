using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class DocRevisionQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "DocumentRevisionId";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public DocumentClass DocClass { get; set; }

        public bool JustPendigModify { get; set; } = false;

        public List<int> DocumentGroupIds { get; set; }
    }
}
