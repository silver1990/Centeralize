using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class ListWaitingPackForReceiptDto
    {
        public long PackId { get; set; }

        public string PackCode { get; set; }

        public long POId { get; set; }

        public string POCode { get; set; }

        public bool IsPart { get; set; }
        public long? subjectProductId { get; set; }

        public string SupplierName { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierLogo { get; set; }

        public long? LogisticDateEnd { get; set; }

        [JsonIgnore]
        public List<ReceiptSubjectDto> ReceiptProducts { get; set; }

        public List<string> Products
        {
            get
            {
                return ReceiptProducts.SelectMany(c => c.PartProductNames.Any() ? c.PartProductNames.Select(v => v.ProductName).ToList() : new List<string> { c.ProductName }).ToList();
            }
        }
        public UserAuditLogDto UserAudit { get; set; }
        public ListWaitingPackForReceiptDto()
        {
            UserAudit = new UserAuditLogDto();
            ReceiptProducts = new List<ReceiptSubjectDto>();
        }

    }
}
