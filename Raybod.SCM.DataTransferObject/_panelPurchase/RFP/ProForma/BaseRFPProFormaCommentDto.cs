using Raybod.SCM.DataTransferObject.RFP.RFPComment;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class BaseRFPProFormaCommentDto
    {
        public long CommentId { get; set; }

        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<RFPCommentAttachmentDto> attachments { get; set; }

        public List<UserMentionInfoDto> UserMentions { get; set; }

        public List<RFPProFormaCommentListDto> ReplayComments { get; set; }

        public BaseRFPProFormaCommentDto()
        {
            attachments = new List<RFPCommentAttachmentDto>();
            ReplayComments = new List<RFPProFormaCommentListDto>();
            UserAudit = new UserAuditLogDto();
            UserMentions = new List<UserMentionInfoDto>();
        }
    }
}
