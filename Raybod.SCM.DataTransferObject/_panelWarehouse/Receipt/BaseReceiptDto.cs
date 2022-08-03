using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class BaseReceiptDto
    {
        public long ReceiptId { get; set; }

        public string ReceiptCode { get; set; }

        public string Note { get; set; }

        public long? PackId { get; set; }

        public int? SupplierId { get; set; }

        public ReceiptStatus ReceiptStatus { get; set; }
                
    }
}
