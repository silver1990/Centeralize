using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class InprogressPOListDto : ListPODto
    {
        public string PRContractCode { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierLogo { get; set; }

        public long PRContractDateIssued { get; set; }

        public long PRContractDateEnd { get; set; }

        public PContractType PContractType { get; set; }

        /// <summary>
        /// اطلاعات ثبت کننده
        /// </summary>
        public UserAuditLogDto UserAudit { get; set; }

        public List<UserMentionDto> ActivityUsers { get; set; }

        public InprogressPOListDto()
        {
            UserAudit = new UserAuditLogDto();
            ActivityUsers = new List<UserMentionDto>();
        }
    }
}
