using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class DocumentRevisionDto : BaseDocumentRevisionDto
    {
        public long Id { get; set; }

        public long DocumentId { get; set; }

        public List<BaseDocumentRevisionAttachmentDto> RevisionAttachments { get; set; }

        public DocumentRevisionDto()
        {
            RevisionAttachments = new List<BaseDocumentRevisionAttachmentDto>();
        }
    }
}
