using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.Communication.NCR
{
    public class NCRQuestionDetailsDto
    {
        public NCRReason NCRReason { get; set; }

        public List<QuestionDto> Questions { get; set; }        

        public NCRQuestionDetailsDto()
        {
            Questions = new List<QuestionDto>();            
        }
    }
}
