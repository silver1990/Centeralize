using Raybod.SCM.DataTransferObject.Document;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelDocument.Communication
{
    public class CreateCommunicateionInfoDto
    {
        public List<RevisionMiniInfoDto> Revisions { get; set; }
        public CurrentContractInfoDto CurrentContractInfo { get; set; }
        public string CustomerName { get; set; }
        public CreateCommunicateionInfoDto()
        {
            Revisions = new List<RevisionMiniInfoDto>();
        }
    }
}
