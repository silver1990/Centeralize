using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.RFP.RFPComment
{
    public class AddRFPCommentDto
    {
        [Required]
        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        public RFPInqueryType InqueryType { get; set; }

        public List<AddAttachmentDto> Attachments { get; set; }

        public List<int> UserIds { get; set; }

        public List<long> RFPInqueryIds { get; set; }
    }
}
