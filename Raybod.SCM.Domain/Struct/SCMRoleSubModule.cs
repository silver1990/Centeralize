using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.Domain.Struct
{
    public struct SCMRoleSubModule
    {
        public const string Document = ",MDR,NCR,Comment,Project,Revision,TQ,Transmittal,Setting,";
        public const string Procurement = ",BOM,MRP,PO,PR,PrContract,RFP,warehouse,Financial,";
        public const string Construction = ",OperationInProgress,OperationList,";
    }
}