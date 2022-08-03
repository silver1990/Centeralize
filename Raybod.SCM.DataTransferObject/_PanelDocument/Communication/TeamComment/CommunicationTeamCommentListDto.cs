using Raybod.SCM.DataTransferObject.User;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class CommunicationTeamCommentListDto
    {
        public long CommentId { get; set; }

        [Required]
        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<CommunicationAttachmentDto> Attachments { get; set; }

        public List<CommunicationTeamCommentListDto> ReplayComments { get; set; }

        public List<UserMiniInfoDto> UserMentions { get; set; }

        public CommunicationTeamCommentListDto()
        {
            Attachments = new List<CommunicationAttachmentDto>();
            ReplayComments = new List<CommunicationTeamCommentListDto>();
            UserAudit = new UserAuditLogDto();
            UserMentions = new List<UserMiniInfoDto>();
        }
    }
}
