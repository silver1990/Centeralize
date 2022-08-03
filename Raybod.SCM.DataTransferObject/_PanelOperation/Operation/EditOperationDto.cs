using Raybod.SCM.DataTransferObject.Operation;
using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Operation
{
    public class EditOperationDto:BaseOperationDto
    {
        public int OperationGroupId { get; set; }
    }
}
