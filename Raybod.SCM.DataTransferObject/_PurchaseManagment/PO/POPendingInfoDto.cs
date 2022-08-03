using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class POPendingInfoDto : BasePODto
    {
        public string SupplierName { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierLogo { get; set; }

        public long PRContractId { get; set; }

        public decimal Tax { get; set; }
        public string ProductGroup { get; set; } = "";
        public string MrpCode { get; set; } = "";

        /// <summary>
        /// مبلغ قرارداد
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal FinalTotalAmount { get; set; }


        /// <summary>
        /// تاریخ ثبت قرارداد
        /// </summary>
        public long PRContractDateIssued { get; set; }

        /// <summary>
        /// تاریخ پایان قرارداد
        /// </summary>
        public long PRContractDateEnd { get; set; }

        /// <summary>
        /// کد قرارداد خرید
        /// </summary>
        public string PRContractCode { get; set; }

        /// <summary>
        /// موضوع سفارش
        /// </summary>
        public List<POPendingSubjectListDto> POSubjects { get; set; }

        /// <summary>
        /// شرایط چرداخت
        /// </summary>
        public List<EditPOTermsOfPaymentDto> POTermsOfPayments { get; set; }


        /// <summary>
        /// اطلاعات ثبت کننده
        /// </summary>
        public UserAuditLogDto UserAudit { get; set; }

        public POPendingInfoDto()
        {
            POSubjects = new List<POPendingSubjectListDto>();
            POTermsOfPayments = new List<EditPOTermsOfPaymentDto>();
            UserAudit = new UserAuditLogDto();
        }
    }
}
