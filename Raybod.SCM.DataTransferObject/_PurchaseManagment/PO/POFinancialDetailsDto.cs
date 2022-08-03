using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class POFinancialDetailsDto
    {
        public string POCode { get; set; }

        public decimal Tax { get; set; }

        public CurrencyType CurrencyType { get; set; }

        /// <summary>
        /// مبلغ سفارش
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal FinalTotalAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalTax { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<POSubjectFinancialDetailsDto> POSubjects { get; set; }
        public POFinancialDetailsDto()
        {
            POSubjects = new List<POSubjectFinancialDetailsDto>();
            UserAudit = new UserAuditLogDto();
        }
    }
}
