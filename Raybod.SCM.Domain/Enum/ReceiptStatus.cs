namespace Raybod.SCM.Domain.Enum
{
    //Receipt
    public enum ReceiptStatus
    {
       
        /// <summary>
        /// کنترل نشده
        /// </summary>
        PendingForQC = 0,

        /// <summary>
        /// برگشت از خرید
        /// </summary>
        QCRejected = 1,

        /// <summary>
        /// 
        /// </summary>
        ConditionalAcceptance = 2,

        /// <summary>
        /// تایید کیفی
        /// </summary>
        QCPassed = 3,

    }
}
