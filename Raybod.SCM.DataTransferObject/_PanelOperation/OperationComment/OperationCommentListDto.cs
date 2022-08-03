using Raybod.SCM.DataTransferObject._PanelOperation;
using Raybod.SCM.DataTransferObject.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.OperationComment
{
    public class OperationCommentListDto
    {
        public long CommentId { get; set; }

        [Required]
        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<OperationAttachmentDto> Attachments { get; set; }

        public List<OperationCommentListDto> ReplayComments { get; set; }

        public List<UserMiniInfoDto> UserMentions { get; set; }

        public OperationCommentListDto()
        {
            Attachments = new List<OperationAttachmentDto>();
            ReplayComments = new List<OperationCommentListDto>();
            UserAudit = new UserAuditLogDto();
            UserMentions = new List<UserMiniInfoDto>();
        }
    }
}
