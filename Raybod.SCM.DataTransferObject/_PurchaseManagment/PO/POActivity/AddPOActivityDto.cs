using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PO.POActivity
{
    public class AddPOActivityDto
    {
        [MaxLength(800)]
        [Required]
        public string Description { get; set; }

        public long? DateEnd { get; set; }
        public double Weight { get; set; }

        public int ActivityOwnerId { get; set; }
    }
}
