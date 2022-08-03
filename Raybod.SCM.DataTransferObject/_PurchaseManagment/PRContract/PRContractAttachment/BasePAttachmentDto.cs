using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class BasePAttachmentDto
    {
        public long Id { get; set; }
        
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
        ///  نام فایل
        /// </summary>
        [MaxLength(250, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string FileSrc { get; set; }

    }
}