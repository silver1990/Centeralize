using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Payment
{
    public class BasePaymentAttachmentDto
    {
        public long Id { get; set; }

        public long PaymentId { get; set; }

        /// <summary>
        /// عنوان فایل
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///  نام فایل
        /// </summary>
        [MaxLength(250, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string FileName { get; set; }

        /// <summary>
        /// حجم فایل
        /// </summary>
        [Required]
        public long FileSize { get; set; }

        /// <summary>
        /// نوع فایل
        /// </summary>
        [Required]
        public string FileType { get; set; }

        /// <summary>
        /// آدرس کامل فایل
        /// </summary>
        public string FileSrc { get; set; }
    }
}
