using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class PurchaseRequestMiniInfoDto
    {
        public long PurchaseRequestId { get; set; }

        /// <summary>
        /// کد قرارداد مرجع
        /// </summary>
        public string BaseContractCode { get; set; }

        /// <summary>
        /// کد درخواست خرید
        /// </summary>
        [Display(Name = "کد درخواست خرید")]
        public string PurchaseRequestCode { get; set; }
        
        /// <summary>
        /// اطلاعات مرجع
        /// </summary>        
        public ReferenceInfo ReferenceInfo { get; set; }
        
        /// <summary>
        ///  وضعیت درخواست خرید
        /// </summary>
        [Display(Name = "وضعیت")]
        public PRStatus Status { get; set; }

        /// <summary>
        /// ثبت درخواست خرید براساس؟
        /// </summary>
        public TypeOfInquiry RFPType { get; set; }

        /// <summary>
        /// نوع صدور درخواست پروپوزال
        /// </summary>
        public PRType Type { get; set; }

        /// <summary>
        /// لاگ کاربران
        /// </summary>
        public UserAuditLogDto UserAudit { get; set; }

        public List<PurchaseRequestItemInfoDto> PurchaseRequestItems { get; set; }
    }
}
