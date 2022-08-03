using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Helper;
using System;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class DocumentQueryDto : IQueryObject
    {
        public string SearchText { get; set; }

        public string SortBy { get; set; } = "DocumentId";

        public bool IsSortAscending { get; set; } = false;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
        public Nullable<bool> IsRequiredTransmittal { get; set; }

        public DocumentClass DocClass { get; set; }

        public List<RevisionStatus> RevisionStatuses { get; set; }

        public CommunicationCommentStatus CommunicationCommentStatus { get; set; }

        public List<int> DocumentGroupIds { get; set; }
        public List<int> AreaIds { get; set; }

        public List<int> ProductIds { get; set; }

    }
}
