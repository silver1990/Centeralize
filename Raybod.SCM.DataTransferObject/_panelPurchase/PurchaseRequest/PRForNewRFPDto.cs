using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class PRForNewRFPDto
    {
        public long Id { get; set; }

        public string ContractCode { get; set; }

        public string PRCode { get; set; }

        public string ProductGroupTitle { get; set; }

        public int ProductGroupId { get; set; }


        public List<PRItemsForNewRFPDTO> PRItems { get; set; }

        public PRForNewRFPDto()
        {
            PRItems = new List<PRItemsForNewRFPDTO>();
        }
    }
}
