using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class WaitingMrpForNewPrDto
    {
        public string ContractCode { get; set; }

        public long? MrpId { get; set; }

        public TypeOfInquiry TypeOfInquiry { get; set; }

        [MaxLength(800)]
        public string Note { get; set; }

        public List<MrpPurchaseRequestItemDto> PurchaseRequestItems { get; set; }

        public WaitingMrpForNewPrDto()
        {
            PurchaseRequestItems = new List<MrpPurchaseRequestItemDto>();
        }
    }
}
