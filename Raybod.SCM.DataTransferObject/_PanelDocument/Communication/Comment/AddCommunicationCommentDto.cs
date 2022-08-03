using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class AddCommunicationCommentDto
    {
        public int CustomerId { get; set; }

        public CommunicationCommentStatus CommentStatus { get; set; }
        public CompanyIssue CompanyIssue { get; set; }

        public List<AddCommentQuestionDto> Questions { get; set; }

        public List<AddRevisionAttachmentDto> Attachments { get; set; }

    }
}
