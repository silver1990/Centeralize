using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class AddRevisionActivityDto
    {
        [MaxLength(800)]
        [Required]
        public string Description { get; set; }

        public long? DateEnd { get; set; }

        public int ActivityOwnerId { get; set; }
    }
}
