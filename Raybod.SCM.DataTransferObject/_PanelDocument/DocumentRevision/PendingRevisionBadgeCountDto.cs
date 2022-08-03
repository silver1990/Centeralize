namespace Raybod.SCM.DataTransferObject.Document
{
    public class PendingRevisionBadgeCountDto
    {
        public int InProgressRevisionCount { get; set; }

        public int PendingConfirmRevisionCount { get; set; }
    }
}
