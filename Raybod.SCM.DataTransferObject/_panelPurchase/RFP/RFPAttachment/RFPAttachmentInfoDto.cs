namespace Raybod.SCM.DataTransferObject.RFP
{
    public class RFPAttachmentInfoDto : BaseRFPAttachmentDto
    {
        public long Id { get; set; }

        /// <summary>
        /// شناسه درخواست پروپوزال
        /// </summary>
        public long? RFPId { get; set; }

    }
}
