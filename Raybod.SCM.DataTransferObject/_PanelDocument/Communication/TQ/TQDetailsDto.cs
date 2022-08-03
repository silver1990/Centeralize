using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.Communication.TQ
{
    public class TQDetailsDto : TQListDto
    {
        public string Subject { get; set; }

        public List<QuestionDto> Questions { get; set; }

        public List<CommunicationAttachmentDto> Attachments { get; set; }

        public TQDetailsDto()
        {
            Questions = new List<QuestionDto>();
            Attachments = new List<CommunicationAttachmentDto>();
        }
    }
}
