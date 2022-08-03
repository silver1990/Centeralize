using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class WaitingPRForNewRFPListDto
    {

        public long Id { get; set; }

        public string ContractCode { get; set; }

        public string ProductGroupTitle { get; set; }

        public int ProductGroupId { get; set; }

        [Required]
        public string PRCode { get; set; }

        /// <summary>
        /// کوچکترین تاریخ شروع
        /// </summary>
        public long DateStart { get; set; }

        /// <summary>
        /// شماره برنامه مود
        /// </summary>
        public string MrpNumber { get; set; }

        /// <summary>
        /// بزرگترین تاریخ شروع
        /// </summary>
        public long DateEnd { get; set; }

        /// <summary>
        /// کالاها
        /// </summary>
        public List<string> Products { get; set; }

        public UserAuditLogDto UserAudit { get; set; }
    }
}
