using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject
{
    public class AddAttachmentDto
    {

        /// <summary>
        ///  نام فایل
        /// </summary>
        [MaxLength(250)]
        public string FileName { get; set; }

        /// <summary>
        ///  نام فایل
        /// </summary>
        [MaxLength(50)]
        public string FileSrc { get; set; }
    }

    public class AddPurchaseRequestAttachmentDto
    {
        public long? Id { get; set; }
        /// <summary>
        ///  نام فایل
        /// </summary>
        [MaxLength(250)]
        public string FileName { get; set; }

        /// <summary>
        ///  نام فایل
        /// </summary>
        [MaxLength(50)]
        public string FileSrc { get; set; }
    }
}
