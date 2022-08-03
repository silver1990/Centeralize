using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.Communication.TQ
{
    public class TQDetailsForCreatePdfDto
    {        
        public string TQCode { get; set; }
       
        public string DocNumber { get; set; }
        
        public string RevisionCode { get; set; }
        
        public string ProjectDescription { get; set; }
        
        public string TQSubject { get; set; }
        
        public string QuestionDescription { get; set; }

        public bool IsReplyed { get; set; }

        public string RegisterQuestionUser { get; set; }

        public DateTime? RegisterQuestionDate { get; set; }

        public string ReplyDescription { get; set; }

        public string RegisterReplyUser { get; set; }

        public DateTime? RegisterReplyDate { get; set; }
        
        public CompanyIssue CompanyIssue { get; set; }
        
        public string CustomerLogo { get; set; }        
        public string CompanyLogo { get; set; }
        public long DocumentId { get; set; }
        public long RevisionId { get; set; }
        public string CompanyIssueName { get; set; }
        public string ReplyerCompany { get; set; }
    }
    
}
