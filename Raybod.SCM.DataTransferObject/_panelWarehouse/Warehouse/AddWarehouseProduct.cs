namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class AddWarehouseProduct
    {
        /// <summary>
        /// شناسه کالا 
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// موجودی اولیه
        /// </summary>
        public decimal InitialStock { get; set; }

        /// <summary>
        /// موجودی در لحظه
        /// </summary>        
        public decimal RealStock { get; set; }

        /// <summary>
        /// موجودی موقت
        /// </summary>
        public decimal TemporaryStock { get; set; }
    }
}
