using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class RevisionConfirmationDetailDto : DocumentRevisionListDto
    {
        public ConfirmationWorkflowDto ConfirmationWorkflowDetails { get; set; }                

        public RevisionConfirmationDetailDto()
        {
            
            ConfirmationWorkflowDetails = new ConfirmationWorkflowDto();
        }
    }
}
