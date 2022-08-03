namespace Raybod.SCM.DataTransferObject.RFP
{
    public class BaseRFPAttachmentDto
    {
        public string FileName { get; set; }

        /// <summary>
        /// حجم فایل
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// نوع فایل
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// آدرس کامل فایل
        /// </summary>
        public string FileSrc { get; set; }
    }
}
