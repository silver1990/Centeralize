using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class AddReceiptRejectDto
    {
        [MaxLength(800)]
        public string Note { get; set; }

        public List<AddReceiptRejectSubjectDto> RejectSubjects { get; set; }

        public List<AddAttachmentDto> Attachments { get; set; }
    }
}
