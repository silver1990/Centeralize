using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Invoice
{
    public class WaitingReceiptAndReceiptRejectForInvoiceListDto
    {
        /// <summary>
        /// شناسه مرجع رسید یا خروج
        /// </summary>
        public long RefrenceId { get; set; }

        public WaitingForInvoiceType WaitingForInvoiceType { get; set; }

        public long POId { get; set; }

        public string POCode { get; set; }

        /// <summary>
        /// نوع فاکتور
        /// </summary>
        public InvoiceType InvoiceType { get; set; }

        /// <summary>
        /// شناسه تامین کننده
        /// </summary>
        public int? SupplierId { get; set; }

        /// <summary>
        /// نام تامین کننده
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// کد تامین کننده
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// لوگو تامین کننده
        /// </summary>
        public string SupplierLogo { get; set; }

        public List<WaitingRefrenceCreateDateAndNumberDto> RefrenceCreateDateAndNumbers { get; set; }

        /// <summary>
        /// کالا ها
        /// </summary>
        public List<string> Products { get; set; }

        public UserAuditLogDto UserAudit { get; set; }
        public WaitingReceiptAndReceiptRejectForInvoiceListDto()
        {
            UserAudit = new UserAuditLogDto();
            RefrenceCreateDateAndNumbers = new List<WaitingRefrenceCreateDateAndNumberDto>();
        }

    }

    public class WaitingReceiptAndForInvoiceListDto
    {
        /// <summary>
        /// شناسه مرجع رسید یا خروج
        /// </summary>
        public long ReceiptId { get; set; }

        public WaitingForInvoiceType WaitingForInvoiceType { get; set; }

        public long POId { get; set; }

        public string POCode { get; set; }

        /// <summary>
        /// نوع فاکتور
        /// </summary>
        public InvoiceType InvoiceType { get; set; }

        /// <summary>
        /// شناسه تامین کننده
        /// </summary>
        public int? SupplierId { get; set; }

        /// <summary>
        /// نام تامین کننده
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// کد تامین کننده
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// لوگو تامین کننده
        /// </summary>
        public string SupplierLogo { get; set; }

        public string ReceiptCode { get; set; }
        public string DispatchCode { get; set; } = "";

        /// <summary>
        /// کالا ها
        /// </summary>
        public List<string> Products { get; set; }

        public UserAuditLogDto UserAudit { get; set; }
        public WaitingReceiptAndForInvoiceListDto()
        {
            UserAudit = new UserAuditLogDto();
        }

    }
}
