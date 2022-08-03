using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RevisionAttachment : BaseEntity
    {
        [Key]
        public long RevisionAttachmentId { get; set; }

        public long? DocumentRevisionId { get; set; }

        public long? RevisionActivityTimesheetId { get; set; }

        public long? RevisionCommentId { get; set; }

        public long? ConfirmationWorkFlowId { get; set; }

        public long? TransmittalId { get; set; }

        public RevisionAttachmentType RevisionAttachmentType { get; set; }

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

        [ForeignKey(nameof(DocumentRevisionId))]
        public virtual DocumentRevision DocumentRevision { get; set; }

        [ForeignKey(nameof(RevisionActivityTimesheetId))]
        public virtual RevisionActivityTimesheet RevisionActivityTimesheet { get; set; }

        [ForeignKey(nameof(RevisionCommentId))]
        public virtual RevisionComment RevisionComment { get; set; }

        [ForeignKey(nameof(ConfirmationWorkFlowId))]
        public virtual ConfirmationWorkFlow ConfirmationWorkFlow { get; set; }

        [ForeignKey(nameof(TransmittalId))]
        public virtual Transmittal Transmittal { get; set; }

    }
}
