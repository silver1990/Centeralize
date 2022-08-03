using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class CommunicationTeamCommentQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; }

        public bool IsSortAscending { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 30;

        public CommunicationType Type { get; set; }

    }
}
