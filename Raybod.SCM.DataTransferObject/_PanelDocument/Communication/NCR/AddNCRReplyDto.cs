using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Document.Communication.NCR
{
    public class AddNCRReplyDto
    {
        public long QuestionId { get; set; }

        public string Description { get; set; }

        public List<AddRevisionAttachmentDto> Attachments { get; set; }
    }
}
