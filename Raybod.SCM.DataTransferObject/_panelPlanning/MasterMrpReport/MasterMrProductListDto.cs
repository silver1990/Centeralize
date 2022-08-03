
using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.MasterMrpReport
{
    public class MasterMrProductListDto
    {
        public int ProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductDescription { get; set; }

        public string ProductTechnicalNumber { get; set; }

        public int ProductGroupId { get; set; }

        public string ProductGroupTitle { get; set; }

        public string Unit { get; set; }

        public decimal Quantity { get; set; }

        public decimal PlannedQuantity { get; set; }

        public MrpItemStatus MrpItemStatus { get; set; }
        public List<MasterMrAreaDto> Areas { get; set; }
        public EngineeringDocumentStatus DocumentStatus { get; set; }

    }
}
