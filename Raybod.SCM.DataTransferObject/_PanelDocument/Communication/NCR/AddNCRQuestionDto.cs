using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.Communication.NCR
{
    public class AddNCRQuestionDto
    {
        public string Description { get; set; }

        public List<AddRevisionAttachmentDto> Attachments { get; set; }
    }
}
