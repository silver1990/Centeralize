using Raybod.SCM.DataTransferObject.RFP;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class AddRFPProFromaDto : BaseRFPProFormaDto
    {
        public List<RFPProFormaAttachmentDto> ProFromaAttachments { get; set; }
    }
}
