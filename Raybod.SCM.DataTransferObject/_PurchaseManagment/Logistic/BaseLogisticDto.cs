using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Logistic
{
    public class BaseLogisticDto
    {
        public long LogisticId { get; set; }

        public long PackId { get; set; }

        public LogisticStep Step { get; set; }

        public long? DateStart { get; set; }

        public long? DateEnd { get; set; }

        public LogisticStatus LogisticStatus { get; set; }

        public UserAuditLogDto CreaterUserAudit { get; set; }

        public UserAuditLogDto ModifierUserAudit { get; set; }
        public BaseLogisticDto()
        {
            CreaterUserAudit = new UserAuditLogDto();
            ModifierUserAudit = new UserAuditLogDto();
        }
    }
}
