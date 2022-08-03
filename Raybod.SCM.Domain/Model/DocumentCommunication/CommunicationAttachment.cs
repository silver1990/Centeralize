using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class CommunicationAttachment : BaseEntity
    {
        [Key]
        public long CommunicationAttachmentId { get; set; }

        public long? DocumentCommunicationId { get; set; }

        public long? DocumentTQNCRId { get; set; }

        public long? CommunicationTeamCommentId { get; set; }

        public long? CommunicationQuestionId { get; set; }

        public long? CommunicationReplyId { get; set; }

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

        [ForeignKey(nameof(DocumentCommunicationId))]
        public virtual DocumentCommunication DocumentCommunication { get; set; }

        [ForeignKey(nameof(DocumentTQNCRId))]
        public virtual DocumentTQNCR DocumentTQNCR { get; set; }

        [ForeignKey(nameof(CommunicationTeamCommentId))]
        public virtual CommunicationTeamComment CommunicationTeamComment { get; set; }

        [ForeignKey(nameof(CommunicationQuestionId))]
        public virtual CommunicationQuestion CommunicationQuestion { get; set; }

        [ForeignKey(nameof(CommunicationReplyId))]
        public virtual CommunicationReply CommunicationReply { get; set; }

    }
}
