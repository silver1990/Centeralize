using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Transmittal
{
    public class PenddingTransmittalListDto
    {
        public long DocumentGroupId { get; set; }

        public string DocumentGroupTitle { get; set; }

        public string DocumentGroupCode { get; set; }

        public List<PendingTransmitalRevisionInfoDo> Revisions { get; set; }

        public PenddingTransmittalListDto()
        {
            Revisions = new List<PendingTransmitalRevisionInfoDo>();
        }
    }
}
