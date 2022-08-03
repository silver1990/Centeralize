using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class RFPInfoDto : BaseRFPDto
    {
        public long Id { get; set; }

        public string ProductGroupTitle { get; set; }

        public int ProductGroupId { get; set; }

        public string RFPNumber { get; set; }

        public string ContractCode { get; set; }
        public RFPStatus Status { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<RFPItemInfoDto> RFPItems { get; set; }

        public List<RFPSupplierInfoDto> Suppliers { get; set; }

        public List<RFPInqueryInfoDto> RfpInqueries { get; set; }

        public List<RFPAttachmentInfoDto> Attachments { get; set; }

        public RFPInfoDto()
        {
            UserAudit = new UserAuditLogDto();
            Suppliers = new List<RFPSupplierInfoDto>();
            RfpInqueries = new List<RFPInqueryInfoDto>();
            RFPItems = new List<RFPItemInfoDto>();
            Attachments = new List<RFPAttachmentInfoDto>();
        }
    }
}
