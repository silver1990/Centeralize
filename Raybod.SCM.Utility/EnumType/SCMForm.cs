namespace Raybod.SCM.Utility.EnumType
{
    public enum SCMForm
    {
        /// <summary>
        /// سفارش از قرارداد
        /// </summary>        
        ContractOrder = 1,

        /// <summary>
        /// بودجه بندی
        /// </summary>   
        Budgeting = 2,

        /// <summary>
        ///  برنامه ریزی
        /// </summary>
        MRP = 3,

        /// <summary>
        /// درخواست خرید
        /// </summary>
        PurchaseRequest = 4,

        /// <summary>
        /// درخواست پروپوزال
        /// </summary>
        RFP = 5,

        /// <summary>
        /// درخواست پروپوزال از تامین کننده
        /// </summary>
        RFPSupplier = 6,

        /// <summary>
        /// قرارداد خرید
        /// </summary>
        PrContract = 7,

        /// <summary>
        /// محموله
        /// </summary>
        PO = 8,

        /// <summary>
        /// بازرسی محموله
        /// </summary>
        BatchInception = 9,

        /// <summary>
        /// بشته بندی
        /// </summary>
        Packing = 10,

        ///// <summary>
        ///// مجوز حمل
        ///// </summary>
        //PackReleasedNote = 11,

        ///// <summary>
        ///// مجوز حمل
        ///// </summary>
        //Transportation = 12,

        ///// <summary>
        ///// مجوز حمل
        ///// </summary>
        //ClearancePort = 13,

        /// <summary>
        /// رسید انبار
        /// </summary>
        Receipt = 14,

        /// <summary>
        /// حواله خروج انبار
        /// </summary>
        ReceiptReject = 15,

        /// <summary>
        /// فاکتور
        /// </summary>
        Invoice = 16,

        /// <summary>
        /// صدور وضعیت
        /// </summary>
        PendingToPayment = 17,

        /// <summary>
        /// پرداخت
        /// </summary>
        Payment = 18,

        /// <summary>
        /// ویرایش مدرک
        /// </summary>
        DocumentRevision=19
    }
}
