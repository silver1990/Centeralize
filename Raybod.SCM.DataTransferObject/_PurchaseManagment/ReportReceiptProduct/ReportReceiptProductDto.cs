using Raybod.SCM.DataTransferObject.Receipt;
using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.ReportReceiptProduct
{
    public class ReportReceiptProductDto
    {
        /// <summary>
        /// شناسه رسید
        /// </summary>
        public long ReceiptId { get; set; }

        /// <summary>
        /// شماره رسید
        /// </summary>
        public string ReceiptCode { get; set; }

        public string PackCode { get; set; }

        public ReceiptStatus ReceiptStatus { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<ReceiptPackSubjectDto> ReceiptSubjects { get; set; }

    }
}
