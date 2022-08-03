namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class PRWaitingListBadgeCountDto
    {
        /// <summary>
        /// تعداد در انتظار ثبت  Pr جدید
        /// </summary>
        public int WaitingForNewPRQuantity { get; set; }

        /// <summary>
        /// تعداد در انتظار تایید
        /// </summary>
        public int WaitingForConfirmQuantity { get; set; }
    }
}
