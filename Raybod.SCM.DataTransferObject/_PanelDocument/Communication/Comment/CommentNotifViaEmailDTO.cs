using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelDocument.Communication.Comment
{
    public class CommentNotifViaEmailDTO
    {
        public string SendDate { get; set; }
        public string Message { get; set; }
        public string Discription { get; set; } = "";
        public string  SenderName { get; set; }
    }
}
