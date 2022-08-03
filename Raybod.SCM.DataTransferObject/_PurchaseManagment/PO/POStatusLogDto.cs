using System;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class POStatusLogDto
    {
        public POStatus POStatus { get; set; }
        
        public  long? DateDone { get; set; }
        
        public bool IsDone { get; set; }

        public bool IsDoing { get; set; }
        
        public string DisplayName { get; set; }
    }
}