using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.MrpItem
{
    public class MrpItemInfoDto : MrpItemInfoForAddDto
    {
        public long Id { get; set; }

        public long MrpId { get; set; }

        public MrpItemStatus MrpItemStatus { get; set; }

    }
}
