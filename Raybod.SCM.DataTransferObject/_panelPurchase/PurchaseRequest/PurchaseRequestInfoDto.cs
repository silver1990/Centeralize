using Raybod.SCM.DataTransferObject._panelPurchase.PurchaseRequestConfirmation;
using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class PurchaseRequestInfoDto : BasePurchaseRequestDto
    {
        public string ProductGroupTitle { get; set; }

        public int ProductGroupId { get; set; }

        [Display(Name = "کد Mrp")]
        public string MrpCode { get; set; }

        [Display(Name = "شرح Mrp")]
        public string MrpDescription { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<PurchaseRequestItemInfoDto> PurchaseRequestItems { get; set; }
        public UserAuditLogDto PurchaseRequestConfirmUserAudit { get; set; }

        public List<BasePRAttachmentDto> Attachments { get; set; }

        public List<PurchaseRequestConfirmationUserWorkFlowDto> PurchaseRequestConfirmationUserWorkFlows { get; set; }

        public PurchasingStream PurchasingStream { get; set; }
        public bool IsEditable { get; set; }
        public PurchaseRequestInfoDto()
        {
            PurchaseRequestItems = new List<PurchaseRequestItemInfoDto>();
            Attachments = new List<BasePRAttachmentDto>();
            PurchaseRequestConfirmationUserWorkFlows = new List<PurchaseRequestConfirmationUserWorkFlowDto>();
        }
    }
}
