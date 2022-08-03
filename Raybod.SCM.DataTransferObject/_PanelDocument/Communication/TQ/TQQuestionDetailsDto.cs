using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.Communication.TQ
{
    public class TQQuestionDetailsDto
    {
        public string Subject { get; set; }

        public List<QuestionDto> Questions { get; set; }

        public TQQuestionDetailsDto()
        {
            Questions = new List<QuestionDto>();
        }
    }
}
