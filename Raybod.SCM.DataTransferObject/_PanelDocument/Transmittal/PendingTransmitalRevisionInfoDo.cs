using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Transmittal
{
    public class PendingTransmitalRevisionInfoDo
    {
        public string DocNumber { get; set; }

        public string DocTitle { get; set; }

        public DocumentClass DocClass { get; set; }

        public POI POI { get; set; }

        public long DocumentId { get; set; }

        public long RevisionId { get; set; }

        public string RevisionCode { get; set; }

        public int? PageNumber { get; set; }

        public string PageSize { get; set; }

        public List<RevisionAttachmentDto> RevisionAttachments { get; set; }
        public PendingTransmitalRevisionInfoDo()
        {
            RevisionAttachments = new List<RevisionAttachmentDto>();
        }
    }
}
