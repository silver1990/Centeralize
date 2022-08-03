using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.Domain.Enum
{
    public enum OperationStatus
    {
        NotStarted = 0,

        InProgress = 1,

        PendingConfirm = 2,

        Confirmed = 3,

        Rejected=4,

    }
}
