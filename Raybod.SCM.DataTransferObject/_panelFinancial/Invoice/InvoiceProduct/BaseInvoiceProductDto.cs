namespace Raybod.SCM.DataTransferObject.Invoice
{
    public class BaseInvoiceProductDto
    {
        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        /// <summary>
        /// قیمت واحد ارزی
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// جمع کل نهایی
        /// </summary>
        public decimal TotalProductAmount { get; set; }
    }
}
