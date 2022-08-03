using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class DocumentViewDto : BaseDocumentDto
    {
        public long DocumentId { get; set; }

        public bool IsActive { get; set; }

        public string ContractCode { get; set; }

        public string DocumentGroupTitle { get; set; }

        public string DocumentGroupCode { get; set; }

        public CommunicationCommentStatus CommunicationCommentStatus { get; set; }

        public LastRevisionDto LastRevision { get; set; }

        public List<ProductInfoDto> DocumentProducts { get; set; }

        public  AreaReadDTO Area { get; set; }

        public DocumentViewDto()
        {
            DocumentProducts = new List<ProductInfoDto>();
            LastRevision = new LastRevisionDto();
        }
    }
}
