using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class EditPrContractServiceDto
    {
        public int ServiceId { get; set; }

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
