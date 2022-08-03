using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class ReceiptSubjectDto
    {
        public string ProductName { get; set; }

        public int ProductId { get; set; }

        public List<ReceiptSubjectDto> PartProductNames { get; set; }
        public ReceiptSubjectDto()
        {
            PartProductNames = new List<ReceiptSubjectDto>();
        }
    }
}
