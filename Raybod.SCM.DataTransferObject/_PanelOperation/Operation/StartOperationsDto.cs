using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Operation
{
    public class StartOperationsDto
    {
        public Guid OperationId { get; set; }
        public long OperationDueDate { get; set; }
    }
}
