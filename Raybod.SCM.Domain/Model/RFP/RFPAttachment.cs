using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RFPAttachment : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// شناسه درخواست پروپوزال
        /// </summary>
        public long? RFPId { get; set; }
        public long? ProFormaId { get; set; }

        public long? RFPInqueryId { get; set; }

        public long? RFPSupplierProposalId { get; set; }

        public long? RFPCommentId { get; set; }

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

        [ForeignKey(nameof(RFPId))]
        public virtual RFP RFP { get; set; }

        [ForeignKey(nameof(ProFormaId))]
        public virtual RFPProForma RFPProForma { get; set; }

        [ForeignKey(nameof(RFPInqueryId))]
        public virtual RFPInquery RFPInquery { get; set; }

        [ForeignKey(nameof(RFPSupplierProposalId))]
        public virtual RFPSupplierProposal RFPSupplierProposal { get; set; }

        [ForeignKey(nameof(RFPCommentId))]
        public virtual RFPComment RFPComment { get; set; }
    }
}
