using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class BaseDocumentDto
    {
        /// <summary>
        /// شماره مدرک
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string DocNumber { get; set; }

        [MaxLength(100)]
        public string ClientDocNumber { get; set; }

        [Required]
        [MaxLength(250)]
        public string DocTitle { get; set; }

        [MaxLength(800)]
        public string DocRemark { get; set; }

        [Required]
        public bool IsRequiredTransmittal { get; set; }

        public int? AreaId { get; set; }
        public DocumentClass DocClass { get; set; }

    }
}
