using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class MrpMiniInfoDto
    {
        public long Id { get; set; }

        public string Description { get; set; }

        public string MrpNumber { get; set; }

        public MrpStatus MrpStatus { get; set; }
    }
}
