using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class OperationAttachment:BaseEntity
    {
        [Key]
        public long OperationAttachmentId { get; set; }

        public Guid? OperationId { get; set; }

        public long? OperationCommentId { get; set; }

        /// <summary>
        ///  نام فایل
        /// </summary>
        [MaxLength(250)]
        public string FileName { get; set; }

        /// <summary>
        ///  عنوان فایل
        /// </summary>
        [MaxLength(250)]
        public string FileSrc { get; set; }

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

        [ForeignKey(nameof(OperationId))]
        public virtual Operation Operation { get; set; }


        [ForeignKey(nameof(OperationCommentId))]
        public virtual OperationComment OperationComment { get; set; }
    }
}
