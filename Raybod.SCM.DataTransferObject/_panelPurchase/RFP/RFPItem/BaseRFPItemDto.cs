using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class BaseRFPItemDto : EditRFPItemDto
    {
        [Required]
        public long DateStart { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public long DateEnd { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }


    }
}
