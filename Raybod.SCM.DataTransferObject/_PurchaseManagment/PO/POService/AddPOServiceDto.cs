using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class AddPOServiceDto
    {
        public int ServiceId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }
    }
}
