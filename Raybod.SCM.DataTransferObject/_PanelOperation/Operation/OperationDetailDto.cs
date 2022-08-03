using Raybod.SCM.DataTransferObject._PanelOperation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Operation
{
    public class OperationDetailDto:BaseOperationDto
    {
        public Guid OperationId { get; set; }


        public string ContractCode { get; set; }

        public string OperationGroupTitle { get; set; }

        public string OperationGroupCode { get; set; }

        public bool IsActive { get; set; }

        public OperationProgressDto OperationProgress { get; set; }

        public UserAuditLogDto WhomStarted { get; set; }
        public UserAuditLogDto UserAudit { get; set; }
       
        public long? DueDate { get; set; }
        public OperationDetailDto()
        {
            WhomStarted = new UserAuditLogDto();
            UserAudit = new UserAuditLogDto();
            
        }
    }
}
