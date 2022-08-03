using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Operation
{
    public class NotStartedOperationListDto
    {
        public Guid OperationId { get; set; }
        public string OperationCode { get; set; }

        public string OperationDescription { get; set; }

    }
}
