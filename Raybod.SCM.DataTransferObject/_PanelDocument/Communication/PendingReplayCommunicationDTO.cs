using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelDocument.Communication
{
    public class PendingReplayCommunicationDTO
    {
        public long PendingCommentReply { get; set; }
        public long PendingTQReply { get; set; }
        public long PendingNCRReply { get; set; }
    }
}
