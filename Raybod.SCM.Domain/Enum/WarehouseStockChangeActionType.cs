namespace Raybod.SCM.Domain.Enum
{
    //WarehouseProductStockLog
    public enum WarehouseStockChangeActionType
    {
        /// <summary>
        /// رسید انبار
        /// </summary>
        addReceipt = 1,

        /// <summary>
        /// برگشت از خرید
        /// </summary>        
        RejectReceipt = 2,

        /// <summary>
        /// خروج از انبار
        /// </summary>
        ExitedTheWarehouse = 3,

        /// <summary>
        /// موجودی اولیه
        /// </summary>
        InitialStock = 4,
    }
}
