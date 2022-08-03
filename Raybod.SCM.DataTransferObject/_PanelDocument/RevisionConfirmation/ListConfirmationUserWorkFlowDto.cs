using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document.RevisionConfirmation
{
    public class ListConfirmationUserWorkFlowDto
    {
        public UserAuditLogDto ConfirmUserAudit { get; set; }

        public string ConfirmNote { get; set; }
        
        public List<ConfirmationUserWorkFlowDto> ConfirmationUserWorkFlows { get; set; }
        public ListConfirmationUserWorkFlowDto()
        {
            ConfirmUserAudit = new UserAuditLogDto();
            ConfirmationUserWorkFlows = new List<ConfirmationUserWorkFlowDto>();
        }
    }
}
