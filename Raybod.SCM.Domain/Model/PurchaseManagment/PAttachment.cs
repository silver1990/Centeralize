using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class PAttachment : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public long? PRContractId { get; set; }

        public long? PurchaseRequestId { get; set; }

        public long? POId { get; set; }

        public long? QualityControlId { get; set; }

        public long? PackId { get; set; }

        public long? LogisticId { get; set; }

        public long? ReceiptId { get; set; }

        public long? ReceiptRejectId { get; set; }

        public long? POCommentId { get; set; }
        public long? PurchaseRequestConfirmWorkFlowId { get; set; }
        public long? PrContractConfirmWorkFlowId { get; set; }
        public long? POInspectionId { get; set; }
        public long? POSupplierDocumentId { get; set; }
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

        [ForeignKey(nameof(PurchaseRequestId))] public virtual PurchaseRequest PurchaseRequest { get; set; }

        [ForeignKey(nameof(PRContractId))] public virtual PRContract PRContract { get; set; }

        [ForeignKey(nameof(POId))] public virtual PO PO { get; set; }

        [ForeignKey(nameof(PackId))] public virtual Pack Pack { get; set; }

        [ForeignKey(nameof(QualityControlId))] public virtual QualityControl QualityControl { get; set; }

        [ForeignKey(nameof(LogisticId))] public virtual Logistic Logistic { get; set; }

        [ForeignKey(nameof(ReceiptId))] public virtual Receipt Receipt { get; set; }

        [ForeignKey(nameof(ReceiptRejectId))] public virtual ReceiptReject ReceiptReject { get; set; }

        [ForeignKey(nameof(POCommentId))] public virtual POComment POComment { get; set; }

        [ForeignKey(nameof(PurchaseRequestConfirmWorkFlowId))]
        public virtual PurchaseConfirmationWorkFlow PurchaseConfirmWorkFlow { get; set; }

        [ForeignKey(nameof(POInspectionId))]
        public virtual POInspection POInspection { get; set; }


        [ForeignKey(nameof(POSupplierDocumentId))]
        public virtual PoSupplierDocument PoSupplierDocument { get; set; }

        [ForeignKey(nameof(PrContractConfirmWorkFlowId))]
        public virtual PrContractConfirmationWorkFlow PrContractConfirmationWorkFlow { get; set; }
    }
}