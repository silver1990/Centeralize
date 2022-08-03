using Raybod.SCM.DataTransferObject.QualityControl;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class ReceiptQualityControlDto : EditQualityControlDto
    {
        public long ReceiptId { get; set; }
        public UserAuditLogDto UserAudit { get; set; }

        public List<BaseQualityControlAttachmentDto> Attachments { get; set; }

        public ReceiptQualityControlDto()
        {
            UserAudit = new UserAuditLogDto();
            Attachments = new List<BaseQualityControlAttachmentDto>();
        }
    }
}
