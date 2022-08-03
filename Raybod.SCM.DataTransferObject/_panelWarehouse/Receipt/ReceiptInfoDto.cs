using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class ReceiptInfoDto : BaseReceiptDto
    {
        public string POCode { get; set; }

        public string SupplierName { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierImage { get; set; }

        public string PackCode { get; set; }
        
        public UserAuditLogDto UserAudit { get; set; }

        public ReceiptQualityControlDto QualityControl { get; set; }

        public List<ReceiptPackSubjectDto> ReceiptSubjects { get; set; }
        public List<ReceiptAttachmentDto> ReceiptAttachments { get; set; }
        public string ProductGroupTitle { get; set; } = "";

        public ReceiptInfoDto()
        {
            ReceiptSubjects = new List<ReceiptPackSubjectDto>();
            UserAudit = new UserAuditLogDto();
            QualityControl = new ReceiptQualityControlDto();
            ReceiptAttachments = new List<ReceiptAttachmentDto>();
        }
    }
}
