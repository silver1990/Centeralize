using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Operation
{
    public class OperationProgressDto
    {
        public OperationStatus OperationStatus { get; set; }
        public double ProgressPercent { get; set; }
    }
}
