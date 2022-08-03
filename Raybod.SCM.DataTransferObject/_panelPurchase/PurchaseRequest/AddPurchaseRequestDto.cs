using Raybod.SCM.DataTransferObject._panelPurchase.PurchaseRequestConfirmation;
using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class AddPurchaseRequestDto
    {

        public long MrpId { get; set; }
        public List<AddPurchaseRequestItemsDto> PurchaseRequestItems { get; set; }
        public List<AddAttachmentDto> PRAttachmentDto { get; set; }

        public AddPurchaseRequestConfirmationDto WorkFlow { get; set; }
    }
}
