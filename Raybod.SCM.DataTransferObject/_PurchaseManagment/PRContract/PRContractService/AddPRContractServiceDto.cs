using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class AddPRContractServiceDto
    {
        public int ServiceId { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PriceUnit { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }

        [Display(Name = "مبلغ کل")]
        public decimal PriceTotal
        {
            get
            {
                return Quantity * PriceUnit;
            }
        }
    }
}
