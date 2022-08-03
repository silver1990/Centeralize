namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class BasePurchaseRequestItemDto : AddPurchaseRequestItemDto
    {
        public long Id { get; set; }

        public long PurchaseRequestId { get; set; }

    }
}
