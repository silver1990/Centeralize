using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document.RevisionConfirmation
{
    public class AddRevisionConfirmationDto
    {
        public long ConfirmationWorkFlowId { get; set; }

        [MaxLength(800)]
        public string ConfirmNote { get; set; }

        public int? RevisionPageNumber { get; set; }

        [MaxLength(10)]
        public string RevisionPageSize { get; set; }

        public List<AddUserConfirmationDto> UserConfirmations { get; set; }

        public List<AddRevConfirmationAttachmentDto> Attachments { get; set; }
    }
}
