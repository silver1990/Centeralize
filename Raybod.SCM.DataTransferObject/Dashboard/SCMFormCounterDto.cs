namespace Raybod.SCM.DataTransferObject.Dashboard
{
    public class SCMFormCounterDto
    {
        public long InProgressRevision { get; set; }

        public long PendingConfirmationRevision { get; set; }

        public long PendingTransmittalRevision { get; set; }

        public long PendingCommunicationReply { get; set; }
        public long PendingCommentReply { get; set; }
        public long PendingTQReply { get; set; }
        public long PendingNCRReply { get; set; }

        public long PendingBOM { get; set; }

        public int PendingMRP { get; set; }

        public long PendingPR { get; set; }

        public long PendingApprovePR { get; set; }

        public long PendingRFP { get; set; }

        public long InprogressRFP { get; set; }

        public long PendingPo { get; set; }

        public long InprogressPO { get; set; }

        public long InprogressOperation { get; set; }
    }
}
