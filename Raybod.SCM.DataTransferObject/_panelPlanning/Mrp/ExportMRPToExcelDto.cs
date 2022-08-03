using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class ExportMRPToExcelDto
    {
        public string ProductCode { get; set; }

        public string ProductDescription { get; set; }

        public string ProductTechnicalNumber { get; set; }

        public string ProductGroupName { get; set; }

        public string Unit { get; set; }

        public decimal FreeQuantityInPO { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public decimal GrossRequirement { get; set; }

        public decimal WarehouseStock { get; set; }

        public decimal ReservedStock { get; set; } = 0;

        public decimal NetRequirement
        {
            get
            {
                return (GrossRequirement - WarehouseStock + ReservedStock) < 0 ? 0 : GrossRequirement - WarehouseStock + ReservedStock;
            }
        }

        public decimal SurplusQuantity { get; set; } = 0;

        public decimal FinalRequirment
        {
            get
            {
                return SurplusQuantity + NetRequirement;
            }
        }

        public decimal PO { get; set; }

        public decimal PR
        {
            get
            {
                return FinalRequirment - PO;
            }
        }

        [Required]
        public string DateStart { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public string DateEnd { get; set; }
    }
}
