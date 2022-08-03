using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Document.RevisionConfirmation
{
    public class AddRevConfirmationAttachmentDto
    {
        public long RevisionAttachmentId { get; set; }
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

        public RevisionAttachmentType AttachType { get; set; }
    }
}
