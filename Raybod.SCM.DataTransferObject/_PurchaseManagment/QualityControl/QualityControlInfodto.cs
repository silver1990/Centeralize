using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.QualityControl
{
    public class QualityControlInfodto : EditQualityControlDto
    {
        public long POPreparationId { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<BaseQualityControlAttachmentDto> Attachments{ get; set; }
    }
}