using System.ComponentModel.DataAnnotations;
using Raybod.SCM.DataTransferObject.Supplier;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class ListPRContractDto
    {
        public long PRContractId { get; set; }

        [Display(Name = "شناسه قرارداد")]
        [MaxLength(60, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string PRContractCode { get; set; }

        [Display(Name = "تاریخ قرار داد")]
        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateIssued { get; set; }

        [Display(Name = "تاریخ پایان قرارداد")]
        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateEnd { get; set; }

        [Display(Name = "وضعیت قرارداد")]
        public PRContractStatus PRContractStatus { get; set; }

        [Display(Name = "شماره درخواست خرید")]
        public string RFPNumber { get; set; }


        public string SupplierName { get; set; }

        public int ProductGroupId { get; set; }

        public string ProductGroupTitle { get; set; }

        public string SupplierCode { get; set; }

        public UserAuditLogDto UserAudit { get; set; }


        public ListPRContractDto()
        {
            UserAudit = new UserAuditLogDto();
        }
    }
    public class ListPendingPRContractDto
    {
        public long PRContractId { get; set; }

        [Display(Name = "شناسه قرارداد")]
        [MaxLength(60, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string PRContractCode { get; set; }

        [Display(Name = "تاریخ قرار داد")]
        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateIssued { get; set; }

        [Display(Name = "تاریخ پایان قرارداد")]
        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateEnd { get; set; }

        [Display(Name = "وضعیت قرارداد")]
        public PRContractStatus PRContractStatus { get; set; }

        [Display(Name = "شماره درخواست خرید")]
        public string RFPNumber { get; set; }


        public string SupplierName { get; set; }

        public int ProductGroupId { get; set; }

        public string ProductGroupTitle { get; set; }

        public string SupplierCode { get; set; }
        public PContractType ContractType { get; set; }
        public UserAuditLogDto UserAudit { get; set; }
        public UserAuditLogDto BallInCourtUser { get; set; }
        public ListPendingPRContractDto()
        {
            UserAudit = new UserAuditLogDto();
            BallInCourtUser = new UserAuditLogDto();
        }
    }
}