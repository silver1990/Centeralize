using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class BaseRevisionAvtivityDto : AddRevisionActivityDto
    {
        public long RevisionActivityId { get; set; }

        public RevisionActivityStatus RevisionActivityStatus { get; set; }

        public string Duration { get; set; }

        public UserAuditLogDto ActivityOwner { get; set; }

        public BaseRevisionAvtivityDto()
        {
            ActivityOwner = new UserAuditLogDto();
        }
    }
}
