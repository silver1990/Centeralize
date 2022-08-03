using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class CommentDetailsDto : CommunicationListDto
    {
        public CommunicationCommentStatus CommentStatus { get; set; }

        public List<CommunicationAttachmentDto> Attachments { get; set; }

        public List<QuestionDto> Questions { get; set; }

        public CommentDetailsDto()
        {
            Questions = new List<QuestionDto>();
            Attachments = new List<CommunicationAttachmentDto>();
        }
    }
}
