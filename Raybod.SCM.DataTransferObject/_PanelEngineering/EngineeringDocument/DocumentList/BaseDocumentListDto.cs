using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class BaseDocumentListDto
    {
        [MaxLength(60)]
        public string ContractCode { get; set; }

        [Required]
        [MaxLength(64)]
        public string DocumentListCode { get; set; }

        public int? ContractSubjectId { get; set; }

        public string Description { get; set; }

        public int ProductId { get; set; }
    }
}
