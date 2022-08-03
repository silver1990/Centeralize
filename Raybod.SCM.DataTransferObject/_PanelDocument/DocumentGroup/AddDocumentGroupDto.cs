using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class AddDocumentGroupDto
    {
        [Required]
        [MaxLength(64)]
        public string DocumentGroupCode { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }
    }
}
