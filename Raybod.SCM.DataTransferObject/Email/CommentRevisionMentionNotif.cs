using Raybod.SCM.DataTransferObject._PanelDocument.Communication.Comment;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Email
{
    public class CommentMentionNotif
    {
        public string FAMessage { get; set; }
        public string ENMessage { get; set; }
        public string LinkUrl { get; set; }
        public string UserFullName { get; set; }
        public List<CommentNotifViaEmailDTO> Comments { get; set; }
        public string CompanyName { get; set; }
        public CommentMentionNotif(string faMessage, string linkUrl, List<CommentNotifViaEmailDTO> comments,string companyName,string enMessage="")
        {
            FAMessage = faMessage;
            ENMessage = enMessage;
            LinkUrl=linkUrl;
            Comments = comments;
            CompanyName = companyName;
        }
    }
}
