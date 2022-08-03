using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PO.POComment
{
    public class POCommentListDto
    {
        public long CommentId { get; set; }

        [Required]
        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<POCommentAttachmentDto> attachments { get; set; }

        public List<UserMentionInfoDto> UserMentions { get; set; }

        public List<POCommentListDto> ReplayComments { get; set; }

        public POCommentListDto()
        {
            attachments = new List<POCommentAttachmentDto>();
            ReplayComments = new List<POCommentListDto>();
            UserAudit = new UserAuditLogDto();
            UserMentions = new List<UserMentionInfoDto>();
        }
    }
}
