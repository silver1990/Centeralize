using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class RFPInqueryInfoDto : BaseRFPInqueryDto
    {
        public long Id { get; set; }
        public DefaultInquery DefaultInquery { get; set; }
        public RFPInqueryType RFPInqueryType { get; set; }
        
    }
}
