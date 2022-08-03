using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document.Communication.NCR
{
    public class AddNCRDto
    {
        [Required]
        public AddNCRQuestionDto Question { get; set; }

        public CompanyIssue CompanyIssue { get; set; }

        public NCRReason NCRReason { get; set; }

        public int CompanyIssueId { get; set; }

        //public List<AddAttachmentDto> Attachments { get; set; }
    }
}
