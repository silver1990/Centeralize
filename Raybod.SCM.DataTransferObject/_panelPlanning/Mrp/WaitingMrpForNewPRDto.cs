using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class WaitingMrpForNewPRDto : MrpInfoDto
    {
        public long DateStart { get; set; }

        public long DateEnd { get; set; }

        public PurchasingStream PurchasingStream { get; set; }
    }
}
