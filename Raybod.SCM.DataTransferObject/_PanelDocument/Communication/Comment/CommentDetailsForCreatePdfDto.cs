using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class CommentDetailsForCreatePdfDto
    {
        public string CommentCode { get; set; }

        public string DocNumber { get; set; }

        public string DocTitle { get; set; }
        
        public string DocClientNumber { get; set; }

        public string RevisionCode { get; set; }

        public string ProjectDescription { get; set; }

        public string TransmittalNumber { get; set; }
        public string TransmittalDate { get; set; }

        public string StartDate { get; set; }
        
        public string EndDate { get; set; }

        public CommunicationCommentStatus CommentStatus { get; set; }
        
        public List<CommentQuestionReplyDto> QuestionReplys { get; set; }
        
        public string CustomerLogo { get; set; }

        public string CompanyLogo { get; set; }

        public CommentDetailsForCreatePdfDto()
        {
            QuestionReplys = new List<CommentQuestionReplyDto>();
        }
    }
}
