using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class ReportConfirmationWorkflowDto
    {
        public long ConfirmationWorkFlowId { get; set; }

        public ConfirmationWorkFlowStatus Status { get; set; }

        public string ConfirmNote { get; set; }

        public int? RevisionPageNumber { get; set; }

        public string RevisionPageSize { get; set; }

        public UserAuditLogDto ConfirmUserAudit { get; set; }

        public List<RevisionAttachmentDto> FinalAttachments { get; set; }

        public List<RevisionAttachmentDto> FinalNativeAttachments { get; set; }

        public List<ConfirmationUserWorkFlowDto> ConfirmationUserWorkFlows { get; set; }

        public ReportConfirmationWorkflowDto()
        {
            FinalAttachments = new List<RevisionAttachmentDto>();
            FinalNativeAttachments = new List<RevisionAttachmentDto>();
            ConfirmUserAudit = new UserAuditLogDto();
            ConfirmationUserWorkFlows = new List<ConfirmationUserWorkFlowDto>();
        }
    }
}
