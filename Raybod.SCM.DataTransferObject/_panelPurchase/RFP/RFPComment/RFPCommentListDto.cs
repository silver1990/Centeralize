using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.RFP.RFPComment
{
    public class RFPCommentListDto
    {
        public long CommentId { get; set; }

        [Required]
        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<RFPCommentAttachmentDto> Attachments { get; set; }

        public List<UserMentionInfoDto> UserMentions { get; set; }
        public List<InqueryMentionDto> InqueryMentions { get; set; }

        public List<RFPCommentListDto> ReplayComments { get; set; }

        public RFPCommentListDto()
        {
            Attachments = new List<RFPCommentAttachmentDto>();
            ReplayComments = new List<RFPCommentListDto>();
            UserAudit = new UserAuditLogDto();
            UserMentions = new List<UserMentionInfoDto>();
            InqueryMentions = new List<InqueryMentionDto>();
        }
    }
    public class RFPProFormaCommentListDto
    {
        public long CommentId { get; set; }

        [Required]
        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<RFPCommentAttachmentDto> Attachments { get; set; }

        public List<UserMentionInfoDto> UserMentions { get; set; }

        public List<RFPCommentListDto> ReplayComments { get; set; }

        public RFPProFormaCommentListDto()
        {
            Attachments = new List<RFPCommentAttachmentDto>();
            ReplayComments = new List<RFPCommentListDto>();
            UserAudit = new UserAuditLogDto();
            UserMentions = new List<UserMentionInfoDto>();
        }
    }
}
