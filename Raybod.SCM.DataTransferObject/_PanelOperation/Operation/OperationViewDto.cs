using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Operation
{
    public class OperationViewDto:BaseOperationDto
    {
        public Guid OperationId { get; set; }


        public string ContractCode { get; set; }

        public string OperationGroupTitle { get; set; }

        public string OperationGroupCode { get; set; }

        public bool IsActive { get; set; }
        public long? DueDate { get; set; }
        public OperationProgressDto OperationProgress { get; set; }


    }
    
}
