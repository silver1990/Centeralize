using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Email
{
    public class RejectRevisionEmailDTO
    {
        public string FAMessage { get; set; }
        public string RejectReason { get; set; }
        public string ENMessage { get; set; }
        public string LinkUrl { get; set; }
        public string UserFullName { get; set; }
        public string CompanyName { get; set; }
        public RejectRevisionEmailDTO(string rejectReason, string linkUrl,string userFullName,string companyName,string faMessage,string enMessage)
        {
            FAMessage = faMessage;
            ENMessage = enMessage;
            RejectReason = rejectReason;
            LinkUrl = linkUrl;
            UserFullName = userFullName;
            CompanyName = companyName;
        }
    }
}
