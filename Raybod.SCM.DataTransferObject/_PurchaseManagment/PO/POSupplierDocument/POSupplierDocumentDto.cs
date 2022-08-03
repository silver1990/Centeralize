using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PO.POSupplierDocuemnt
{
    public class POSupplierDocumentDto
    {
        public long POSupplierDocumentId { get; set; }
        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string ProductUnit { get; set; }

        public string ProductGroupName { get; set; }

        public string TechnicalNumber { get; set; }
        public int ProductId { get; set; }
        public string DocumentTitle { get; set; }
        public string DocumentCode { get; set; }
        public UserAuditLogDto UserAudit { get; set; }
        public List<POSupplierDocumentAttachmentDto> Attachments { get; set; }
        public POSupplierDocumentDto()
        {
         
            Attachments = new List<POSupplierDocumentAttachmentDto>();
        }
    }
}
