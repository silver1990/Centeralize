using Raybod.SCM.DataTransferObject.QualityControl;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Packing
{
    public class PackingQualityControlInfodto : EditQualityControlDto
    {
        public long PackId { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<BaseQualityControlAttachmentDto> Attachments { get; set; }
    }
}
