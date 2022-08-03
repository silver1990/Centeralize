using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class CommunicationQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; }

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public DocumentClass DocClass { get; set; }

        public CommunicationType CommunicationType { get; set; }

        public DocumentCommunicationStatus CommunicationStatus { get; set; }

        public CompanyIssue CompanyIssue { get; set; }

        public int CompanyIssueId { get; set; }

        public List<int> DocumentGroupIds { get; set; }

    }
}
