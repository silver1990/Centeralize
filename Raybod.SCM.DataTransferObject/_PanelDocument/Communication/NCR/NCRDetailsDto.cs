using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.Communication.NCR
{
    public class NCRDetailsDto : NCRListDto
    {
        public NCRReason NCRReason { get; set; }

        public List<QuestionDto> Questions { get; set; }

        public List<CommunicationAttachmentDto> Attachments { get; set; }

        public NCRDetailsDto()
        {
            Questions = new List<QuestionDto>();
            Attachments = new List<CommunicationAttachmentDto>();
        }
    }
}
