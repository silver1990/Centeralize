using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.MrpItem
{
    public class EditMrpItemDto
    {
        public decimal GrossRequirement { get; set; }

        public decimal WarehouseStock { get; set; }

        public decimal ReservedStock { get; set; }

        public decimal NetRequirement
        {
            get
            {
                return (GrossRequirement - WarehouseStock + ReservedStock) < 0 ? 0 : GrossRequirement - WarehouseStock + ReservedStock;
            }
        }

        public decimal SurplusQuantity { get; set; }

        public decimal FinalRequirment
        {
            get
            {
                return SurplusQuantity + NetRequirement;
            }
        }

        [Required]
        public long DateStart { get; set; }


        public long DateEnd { get; set; }
    }
}
