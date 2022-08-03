
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class MasterMRForExportToExcelDto
    {
        public int ProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductUnit { get; set; }

        public string ProductDescription { get; set; }

        public string ProductTechnicalNumber { get; set; }

        public string ProductGroupTitle { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal GrossRequirement { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal WarehouseStock { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ReservedStock { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal FreeQuantityInPO { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetRequirement
        {
            get
            {
                return (GrossRequirement - WarehouseStock + ReservedStock) < 0 ? 0 : GrossRequirement - WarehouseStock + ReservedStock;
            }
        }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SurplusQuantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal FinalRequirment
        {
            get
            {
                return SurplusQuantity + NetRequirement;
            }
        }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PR
        {
            get
            {
                return FinalRequirment;
            }
        }

        public string DateStart { get; set; }

        public string DateEnd { get; set; }
    }
}
