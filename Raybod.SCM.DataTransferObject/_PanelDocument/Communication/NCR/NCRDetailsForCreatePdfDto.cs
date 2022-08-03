using Raybod.SCM.Domain.Enum;
using System;

namespace Raybod.SCM.DataTransferObject.Document.Communication.NCR
{
    public class NCRDetailsForCreatePdfDto
    {
        public string NCRCode { get; set; }

        public string CompanyName { get; set; }

        public string DocNumber { get; set; }

        public string RevisionCode { get; set; }

        public string ProjectDescription { get; set; }

        public NCRReason NCRReason { get; set; }

        public string QuestionDescription { get; set; }

        public bool IsReplyed { get; set; }

        public string RegisterQuestionUser { get; set; }

        public DateTime? RegisterQuestionDate { get; set; }

        public string ReplyDescription { get; set; }

        public string RegisterReplyUser { get; set; }

        public DateTime? RegisterReplyDate { get; set; }

        public CompanyIssue CompanyIssue { get; set; }

        public string CompanyLogo { get; set; }

        public string CustomerLogo { get; set; }
        public long DocumentId { get; set; }
        public long RevisionId { get; set; }
        public string CompanyIssueName { get; set; }
        public string ReplyerCompany { get; set; }
    }
}
