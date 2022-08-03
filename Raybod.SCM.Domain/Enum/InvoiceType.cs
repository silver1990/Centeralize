namespace Raybod.SCM.Domain.Enum
{
    //Invoice
    public enum InvoiceType
    {
        /// <summary>
        /// خرید داخلی
        /// </summary>
        InternalPurchase = 1,

        /// <summary>
        /// خرید خارجی
        /// </summary>
        ExternalPurchase = 2,

        /// <summary>
        /// برگشت از خرید
        /// </summary>
        RejectPurchase = 3,
    }
}
