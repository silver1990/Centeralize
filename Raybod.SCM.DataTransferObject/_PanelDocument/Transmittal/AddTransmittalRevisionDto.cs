using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Transmittal
{
    public class AddTransmittalRevisionDto
    {
        public long DocumentRevisionId { get; set; }

        public POI POI { get; set; }
    }
}
