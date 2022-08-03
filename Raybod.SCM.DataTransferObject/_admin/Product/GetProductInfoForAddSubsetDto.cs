using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Product
{
    public class GetProductInfoForAddSubsetDto
    {

        public string ProductCode { get; set; }
        public string Description { get; set; }
        public string TechnicalNumber { get; set; }
        public string Unit { get; set; }
        public string ProductGroupTitle { get; set; }
        public decimal CoefficientUse { get; set; }
        public MaterialType MaterialType { get; set; }
        public bool IsRequiredMRP { get; set; }
        public AreaReadDTO Area { get; set; }
        public int? ProductId { get; set; }
        public List<ValidProductForAddingSubsetDto> Products { get; set; }
        public GetProductInfoForAddSubsetDto()
        {
            Products = new List<ValidProductForAddingSubsetDto>();
        }
    }
}
