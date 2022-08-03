using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Document.Communication
{
    public class CommentTQNCRAttachmentDto
    {
        public List<BasicAttachmentDownloadDto> CommentAttachment { get; set; }
        public List<BasicAttachmentDownloadDto> ReplyAttachment { get; set; }
        public CommentTQNCRAttachmentDto()
        {
            CommentAttachment = new List<BasicAttachmentDownloadDto>();
            ReplyAttachment = new List<BasicAttachmentDownloadDto>();
        }
    }
    public class BasicAttachmentDownloadDto
    {
        public string FileName { get; set; }
        public string FileSrc { get; set; }
    }
    
}
