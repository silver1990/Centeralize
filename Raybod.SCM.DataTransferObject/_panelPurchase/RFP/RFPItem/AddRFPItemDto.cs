using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class AddRFPItemDto
    {
        public long PurchaseRequestId { get; set; }

        public int ProductId { get; set; }
        
    }
}
