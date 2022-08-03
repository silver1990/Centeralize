using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.MrpItem
{
    public class BaseMrpItemDto
    {
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
        public long DateStart { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateEnd { get; set; }
    }
}
