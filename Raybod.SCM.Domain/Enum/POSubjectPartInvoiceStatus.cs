namespace Raybod.SCM.Domain.Enum
{
    //POStatus
    public enum POSubjectPartInvoiceStatus
    {
        /// <summary>
        /// فاکتور ثبت نشده
        /// </summary>
        NotReadyForInvoice = 0,

        /// <summary>
        /// در انتظار ثبت فاکتور
        /// </summary>
        WaitingForInvoice = 1,

        /// <summary>
        /// فاکتور ثبت شد
        /// </summary>
        CreatedInvoice = 2
    }
}
