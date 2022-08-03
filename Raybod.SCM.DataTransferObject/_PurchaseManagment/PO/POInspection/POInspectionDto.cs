using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PO.POInspection
{
    public class POInspectionDto
    {
        public long POInspectionId { get; set; }
        public string Description { get; set; }
        public string ResultNote { get; set; }
        public long? DueDate { get; set; }
        public InspectionResult Result { get; set; }
        public UserAuditLogDto Inspector { get; set; }
        public UserAuditLogDto UserAudit { get; set; }
        public List<POInspectionAttachmentDto> Attachments { get; set; }
        public POInspectionDto()
        {
         
            Attachments = new List<POInspectionAttachmentDto>();
        }
    }
}
