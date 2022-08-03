using Raybod.SCM.DataTransferObject.PurchaseRequest;
using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class PurchaseRequestEditInfoDto : BasePurchaseRequestDto
    {
        public string ProductGroupTitle { get; set; }

        public int ProductGroupId { get; set; }

        [Display(Name = "کد Mrp")]
        public string MrpCode { get; set; }

        [Display(Name = "شرح Mrp")]
        public string MrpDescription { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<EditPurchaseRequestItemInfoDto> PurchaseRequestItems { get; set; }
        public UserAuditLogDto PurchaseRequestConfirmUserAudit { get; set; }

        public List<BasePRAttachmentDto> Attachments { get; set; }

        public bool IsEditable { get; set; }

        public PurchasingStream PurchasingStream { get; set; }
        public PurchaseRequestEditInfoDto()
        {
            PurchaseRequestItems = new List<EditPurchaseRequestItemInfoDto>();
            Attachments = new List<BasePRAttachmentDto>();        }
    }
}
