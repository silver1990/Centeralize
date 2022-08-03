using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class ListReceiptDto : BaseReceiptDto
    {
        public string SupplierName { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierImage { get; set; }

        public string PackCode { get; set; }
        [JsonIgnore]
        public List<ReceiptSubjectDto> ReceiptProducts { get; set; }

        public List<string> Products
        {
            get
            {
                return ReceiptProducts.SelectMany(c => c.PartProductNames.Any() ? c.PartProductNames.Select(a => a.ProductName).ToList() : new List<string> { c.ProductName }).ToList();
            }
        }

        public UserAuditLogDto UserAudit { get; set; }

        public ListReceiptDto()
        {
            UserAudit = new UserAuditLogDto();
            ReceiptProducts = new List<ReceiptSubjectDto>();
        }
    }
}
