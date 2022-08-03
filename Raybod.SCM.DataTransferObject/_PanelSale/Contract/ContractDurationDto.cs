using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class ContractDurationDto
    {
        public long? DateIssued { get; set; }


        public long? DateEffective { get; set; }


        public long? DateEnd { get; set; }

        public int? ContractDuration { get; set; }
    }
}
