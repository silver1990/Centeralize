namespace Raybod.SCM.DataTransferObject.RFP
{
    public class RFPInqueryAttachmentDto : BaseRFPAttachmentDto
    {
        public long Id { get; set; }

        public long? RFPInqueryId { get; set; }
    }
}
