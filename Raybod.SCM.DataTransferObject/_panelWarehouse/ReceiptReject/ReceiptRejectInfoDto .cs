using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class ReceiptRejectInfoDto
    {
        public long ReceiptRejectId { get; set; }

        public long ReceiptId { get; set; }

        public string ReceiptCode { get; set; }

        public string ReceiptRejectCode { get; set; }

        public string POCode { get; set; }

        public string SupplierName { get; set; }

        public string Note { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierImage { get; set; }

        public long? DateReceipted { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public ReceiptQualityControlDto QualityControl { get; set; }

        public List<ReceiptRejectSubjectDto> ReceiptRejectSubjects { get; set; }
        public List<ReceiptAttachmentDto> ReceiptRejectAttachments { get; set; }

        public ReceiptRejectInfoDto()
        {
            ReceiptRejectSubjects = new List<ReceiptRejectSubjectDto>();
            UserAudit = new UserAuditLogDto();
            QualityControl = new ReceiptQualityControlDto();
            ReceiptRejectAttachments = new List<ReceiptAttachmentDto>();
        }
    }
}
