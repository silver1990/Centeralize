using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class DocumentArchiveInfoDto : BaseDocumentDto
    {
        public long DocumentId { get; set; }

        public bool IsActive { get; set; }

        public string ContractCode { get; set; }

        public string DocumentGroupTitle { get; set; }

        public string DocumentGroupCode { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<RevisionArchiveDto> Revisions { get; set; }

        public DocumentArchiveInfoDto()
        {
            Revisions = new List<RevisionArchiveDto>();
            UserAudit = new UserAuditLogDto();
        }

    }
}
