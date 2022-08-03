namespace Raybod.SCM.Domain.Enum
{
    //no db
    public enum FinancialAccountType
    {
        /// <summary>
        /// خرید
        /// </summary>
        Purchase = 1,

        /// <summary>
        /// برگشت از خرید
        /// </summary>
        RejectPurchase = 2,

        /// <summary>
        /// پرداخت
        /// </summary>
        Payment = 3,
        
        /// <summary>
        /// موجودی حساب اول سال
        /// </summary>
        InitialAccountOfYear = 4,
    }
}
