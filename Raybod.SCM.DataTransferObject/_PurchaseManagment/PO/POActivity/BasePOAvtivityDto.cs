using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.PO.POActivity
{
    public class BasePOAvtivityDto : AddPOActivityDto
    {
        public long POActivityId { get; set; }

        public POActivityStatus ActivityStatus { get; set; }


        public string Duration { get; set; }

        public UserAuditLogDto ActivityOwner { get; set; }
        public double ProgressPercent { get; set; }
        public double PoProgressPercent { get; set; }
        public BasePOAvtivityDto()
        {
            ActivityOwner = new UserAuditLogDto();
        }
    }
}
