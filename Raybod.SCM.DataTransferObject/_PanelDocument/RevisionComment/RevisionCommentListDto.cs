using Raybod.SCM.DataTransferObject.User;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class RevisionCommentListDto
    {
        public long CommentId { get; set; }

        [Required]
        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<RevisionAttachmentDto> Attachments { get; set; }

        public List<RevisionCommentListDto> ReplayComments { get; set; }

        public List<UserMiniInfoDto> UserMentions { get; set; }

        public RevisionCommentListDto()
        {
            Attachments = new List<RevisionAttachmentDto>();
            ReplayComments = new List<RevisionCommentListDto>();
            UserAudit = new UserAuditLogDto();
            UserMentions = new List<UserMiniInfoDto>();
        }
    }
}
