using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document.Communication.TQ
{
    public class AddTQDto
    {
        public string Subject { get; set; }

        public CompanyIssue CompanyIssue { get; set; }

        public int CompanyIssueId { get; set; }

        [Required]
        public AddTQQuestionDto Question { get; set; }

        public List<AddRevisionAttachmentDto> Attachments { get; set; }
    }
}
