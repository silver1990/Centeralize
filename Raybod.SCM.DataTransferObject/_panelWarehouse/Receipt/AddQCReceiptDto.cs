using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class AddQCReceiptDto
    {
        [MaxLength(800)]
        public string Note { get; set; }

        public List<AddAttachmentDto> Attachments { get; set; }

        [Required]
        public List<AddReceiptProductDto> ReceiptSubjects { get; set; }
    }
}
