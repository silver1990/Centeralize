using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.DataTransferObject.Document;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelDocument.DocumentRevision.Archive
{

    public class DocumentArchiveInfoForCustomerUserDto : BaseDocumentDto
    {
        public long DocumentId { get; set; }

        public bool IsActive { get; set; }

        public string ContractCode { get; set; }

        public string DocumentGroupTitle { get; set; }

        public string DocumentGroupCode { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<RevisionArchiveDto> Revisions { get; set; }
        public AreaReadDTO Area { get; set; }
        public DocumentArchiveInfoForCustomerUserDto()
        {
            Revisions = new List<RevisionArchiveDto>();
            UserAudit = new UserAuditLogDto();
        }

    }
}
