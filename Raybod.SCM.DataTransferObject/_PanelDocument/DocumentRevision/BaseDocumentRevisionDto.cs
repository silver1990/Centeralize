using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class BaseDocumentRevisionDto
    {
        [Required]
        [MaxLength(64)]
        public string DocumentRevisionCode { get; set; }

        [MaxLength(300)]
        public string Reason { get; set; }

        [MaxLength(800)]
        public string Description { get; set; }

        public RevisionStatus RevisionStatus { get; set; }

        public long? DateEnd { get; set; }
    }
}
