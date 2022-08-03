using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class PaymentAttachment : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public string Title { get; set; }

        [Required]
        [MaxLength(300)]
        public string FileName { get; set; }

        /// <summary>
        ///  عنوان فایل
        /// </summary>
        [MaxLength(250)]
        public string FileSrc { get; set; }

        public string FileType { get; set; }

        public long FileSize { get; set; }

        public long? PaymentId { get; set; }

        public long? InvoiceId { get; set; }
        public long? PaymanetConfirmationWorkFlowId { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public Invoice Invoice { get; set; }
        [ForeignKey(nameof(PaymanetConfirmationWorkFlowId))]
        public PaymentConfirmationWorkFlow PaymentConfirmationWorkFlow { get; set; }
    }
}
