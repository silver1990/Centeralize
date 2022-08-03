using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelDocument.Communication
{
    public class RevisionMiniInfoForCusomerUserDto
    {
        public long RevisionId { get; set; }

        public string RevisionCode { get; set; }

        public string DocNumber { get; set; }

        public string DocTitle { get; set; }
        public CurrentContractInfoDto CurrentContractInfo { get; set; }
        public DocumentClass DocClass { get; set; }

        public string DocumentGroupTitle { get; set; }
        public string DocumentGroupCode { get; set; }
        public string TransmittalNumber { get; set; }
        public string TransmittalDate { get; set; }
        public string CustomerName { get; set; }
    }
    
}
