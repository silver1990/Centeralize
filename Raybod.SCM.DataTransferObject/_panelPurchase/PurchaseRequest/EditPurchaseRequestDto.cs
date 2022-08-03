using Raybod.SCM.DataTransferObject._panelPurchase.PurchaseRequestConfirmation;
using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class EditPurchaseRequestDto
    {
       

        [MaxLength(800)]
        public string Note { get; set; }

        public List<AddPurchaseRequestItemDto> PurchaseRequestItems { get; set; }

        public List<AddPurchaseRequestAttachmentDto> PRAttachments { get; set; }
        public AddPurchaseRequestConfirmationDto WorkFlow { get; set; }
    }

    public class EditPurchaseRequestBySysAdminDto
    {



        public List<AddPurchaseRequestItemsSysAdminDto> PurchaseRequestItems { get; set; }

        public List<AddPurchaseRequestAttachmentDto> PRAttachments { get; set; }
    }
}
