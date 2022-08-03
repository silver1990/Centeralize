using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelDocument.Document
{
    public class PendingDocumentForCommentDto:BaseDocumentDto
    {

        public long DocumentId { get; set; }

        public bool IsActive { get; set; }

        public string ContractCode { get; set; }

        public string DocumentGroupTitle { get; set; }

        public string DocumentGroupCode { get; set; }

        public CommunicationCommentStatus CommunicationCommentStatus { get; set; }

        public LastRevisionDto LastRevision { get; set; }
        public AreaReadDTO Area { get; set; }
        public string TransmittalNumber { get; set; }
        public string TransmittalDate { get; set; }
        public PendingDocumentForCommentDto()
        {

            LastRevision = new LastRevisionDto();
        }

    }

    public class PendingDocumentsForCommentDto : BaseDocumentDto
    {

        public long DocumentId { get; set; }

        public bool IsActive { get; set; }

        public string ContractCode { get; set; }

        public string DocumentGroupTitle { get; set; }

        public string DocumentGroupCode { get; set; }

        public CommunicationCommentStatus CommunicationCommentStatus { get; set; }

        public List<LastRevisionDto> LastRevisions { get; set; }
        public AreaReadDTO Area { get; set; }
        public string TransmittalNumber { get; set; }
        public string TransmittalDate { get; set; }
        public PendingDocumentsForCommentDto()
        {

            LastRevisions = new List<LastRevisionDto>();
        }

    }
}
