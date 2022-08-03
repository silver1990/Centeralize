namespace Raybod.SCM.DataTransferObject.RFP.RFPComment
{
    public class RFPCommentAttachmentDto : BaseRFPAttachmentDto
    {
        public long Id { get; set; }

        public long? RFPCommentId { get; set; }
    }
}
