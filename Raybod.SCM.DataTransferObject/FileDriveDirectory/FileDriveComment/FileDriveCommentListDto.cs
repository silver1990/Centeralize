using Raybod.SCM.DataTransferObject.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.FileDriveDirectory
{
    public class FileDriveCommentListDto
    {
        public long CommentId { get; set; }

        [Required]
        public string Message { get; set; }

        public long? ParentCommentId { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<FileDriveCommentAttachmentDto> Attachments { get; set; }

        public List<FileDriveCommentListDto> ReplayComments { get; set; }

        public List<UserMiniInfoDto> UserMentions { get; set; }

        public FileDriveCommentListDto()
        {
            Attachments = new List<FileDriveCommentAttachmentDto>();
            ReplayComments = new List<FileDriveCommentListDto>();
            UserAudit = new UserAuditLogDto();
            UserMentions = new List<UserMiniInfoDto>();
        }
    }
}
