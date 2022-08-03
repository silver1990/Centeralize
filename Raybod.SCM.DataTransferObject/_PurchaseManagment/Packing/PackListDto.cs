using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Packing
{
    public class PackListDto
    {
        public long PackId { get; set; }

        public string PackCode { get; set; }

        public PackStatus PackStatus { get; set; }

        public long? DateCreated { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public PackListDto()
        {
            UserAudit = new UserAuditLogDto();
        }
    }
}
