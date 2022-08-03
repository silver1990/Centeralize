using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class WaitingContractFotMrpDto
    {
        [Display(Name = "کد قرارداد")]
        public string ContractCode { get; set; }

        [Display(Name = "شماره قرارداد")]
        public string ContractNumber { get; set; }

        [Display(Name = "کد قرارداد")]
        public string Description { get; set; }

        public int ProductGroupId { get; set; }

        public string ProductGroupCode { get; set; }

        public string ProductGroupTitle { get; set; }

        /// <summary>
        /// تعداد باقی مانده
        /// </summary>
        public decimal RemainedItem { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public WaitingContractFotMrpDto()
        {
            UserAudit = new UserAuditLogDto();
        }
    }
}
