namespace Raybod.SCM.DataTransferObject.PO.POComment
{
    public class POCommentAttachmentDto : BasePOAttachmentDto
    {
        public long Id { get; set; }

        public long? POCommentId { get; set; }
    }
}
