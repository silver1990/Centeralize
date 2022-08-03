using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Operation
{
    public class StartedOperationViewDto : BaseOperationDto
    {
        public Guid OperationId { get; set; }


        public string ContractCode { get; set; }

        public string OperationGroupTitle { get; set; }

        public string OperationGroupCode { get; set; }

        public bool IsActive { get; set; }

        public OperationProgressDto OperationProgress { get; set; }

        public UserAuditLogDto WhomStarted { get; set; }
        public long? DueDate { get; set; }
        public StartedOperationViewDto()
        {
            WhomStarted = new UserAuditLogDto();
        }
    }
}
