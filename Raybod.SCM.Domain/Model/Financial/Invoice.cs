using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class Invoice : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        [Required]
        //[Column(TypeName = "varchar(64)")]
        [MaxLength(64)]
        public string InvoiceNumber { get; set; }

        public int SupplierId { get; set; }

        public long? POId { get; set; }

        public InvoiceType InvoiceType { get; set; }

        public PContractType PContractType { get; set; }

        /// <summary>
        /// واحد پول قرارداد
        /// </summary>
        public CurrencyType CurrencyType { get; set; }

        /// <summary>
        /// درصد مالیات
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Tax { get; set; } = 9;

        /// <summary>
        /// مجموع کل
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalProductAmount { get; set; }

        /// <summary>
        /// دیگر هزینه ها
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OtherCosts { get; set; }

        /// <summary>
        /// مجموع تخفیف ها
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// مجموع مالیات 
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalTax { get; set; }

        /// <summary>
        /// جمع کل فاکتور
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalInvoiceAmount { get; set; }

        [MaxLength(800)]
        public string Note { get; set; }

        public InvoiceStatus InvoiceStatus { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual ICollection<Receipt> Receipts { get; set; }

        public virtual FinancialAccount FinancialAccount { get; set; }

        public virtual ReceiptReject ReceiptReject { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier Supplier { get; set; }

        [ForeignKey(nameof(POId))]
        public virtual PO PO { get; set; }

        public virtual ICollection<InvoiceProduct> InvoiceProducts { get; set; }

        public virtual ICollection<PaymentAttachment> Attachments { get; set; }
    }
}
