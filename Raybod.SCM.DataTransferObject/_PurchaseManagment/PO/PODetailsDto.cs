using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class PODetailsDto : BasePODto
    {
        /// <summary>
        /// کد قرارداد خرید
        /// </summary>
        public string PRContractCode { get; set; }

        public long PRContractId { get; set; }

        public int SupplierId { get; set; }

        public string SupplierName { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierLogo { get; set; }

        public long PRContractDateIssued { get; set; }

        public long PRContractDateEnd { get; set; }

        public List<POSubjectInfoDto> POSubjects { get; set; }
        /// <summary>
        /// اطلاعات ثبت کننده
        /// </summary>
        public UserAuditLogDto UserAudit { get; set; }
        public double PoProgressPercent { get; set; }

        public PODetailsDto()
        {
            UserAudit = new UserAuditLogDto();
            POSubjects = new List<POSubjectInfoDto>();
        }
    }
    public class PODetailsForCancelDto : BasePODto
    {
        /// <summary>
        /// کد قرارداد خرید
        /// </summary>
        public string PRContractCode { get; set; }

        public long PRContractId { get; set; }

        public int SupplierId { get; set; }

        public string SupplierName { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierLogo { get; set; }

        public long PRContractDateIssued { get; set; }

        public long PRContractDateEnd { get; set; }

        /// <summary>
        /// اطلاعات ثبت کننده
        /// </summary>
        public UserAuditLogDto UserAudit { get; set; }
        public double PoProgressPercent { get; set; }
        public PODetailsForCancelDto()
        {
            UserAudit = new UserAuditLogDto();
        }
    }
}