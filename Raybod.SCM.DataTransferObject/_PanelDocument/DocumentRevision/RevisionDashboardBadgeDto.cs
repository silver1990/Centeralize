namespace Raybod.SCM.DataTransferObject.Document
{
    public class RevisionDashboardBadgeDto
    {
        public int InProgressRevision { get; set; }

        public int PendingConfirmationRevision { get; set; }

        public int PendingTransmittalRevision { get; set; }

    }
}
