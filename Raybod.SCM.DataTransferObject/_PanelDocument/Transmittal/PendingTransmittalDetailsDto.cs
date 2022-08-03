using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Transmittal
{
    public class PendingTransmittalDetailsDto
    {
        public long DocumentGroupId { get; set; }

        public string DocumentGroupTitle { get; set; }

        public string DocumentGroupCode { get; set; }

        public string ContractDescription { get; set; }

        public List<PendingTransmitalRevisionInfoDo> Revisions { get; set; }

        public PendingTransmittalDetailsDto()
        {
            Revisions = new List<PendingTransmitalRevisionInfoDo>();
        }
    }
}
