using Raybod.SCM.DataTransferObject.PRContract;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class BasePRAttachmentDto : BasePAttachmentDto
    {
        public long PurchaseRequestId { get; set; }
    }
}
