using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class CommnetQuestionDetailsDto
    {
        public CommunicationCommentStatus CommentStatus { get; set; }

        //public UserAuditLogDto UserAudit { get; set; }

        public List<QuestionDto> Questions { get; set; }

        public CommnetQuestionDetailsDto()
        {
            //UserAudit = new UserAuditLogDto();
            Questions = new List<QuestionDto>();
        }

    }
}
