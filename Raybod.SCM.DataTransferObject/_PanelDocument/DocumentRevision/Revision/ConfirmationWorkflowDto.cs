using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class ConfirmationWorkflowDto
    {
        public string ConfirmNote { get; set; }

        public int? RevisionPageNumber { get; set; }

        public string RevisionPageSize { get; set; }

        public UserAuditLogDto ConfirmUserAudit { get; set; }

        public List<RevisionAttachmentDto> FinalAttachments { get; set; }

        public List<RevisionAttachmentDto> FinalNativeAttachments { get; set; }

        public List<ConfirmationUserWorkFlowDto> ConfirmationUserWorkFlows { get; set; }

        public ConfirmationWorkflowDto()
        {
            ConfirmUserAudit = new UserAuditLogDto();
            FinalAttachments = new List<RevisionAttachmentDto>();
            FinalNativeAttachments = new List<RevisionAttachmentDto>();
            ConfirmationUserWorkFlows = new List<ConfirmationUserWorkFlowDto>();
        }
    }
}
