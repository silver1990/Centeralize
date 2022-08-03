using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.Domain.Model
{
    public class PO : BaseEntity
    {
        [Key]
        public long POId { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string BaseContractCode { get; set; }

        /// <summary>
        /// کد سفارش
        /// </summary>
        public string POCode { get; set; }

        /// <summary>
        /// مکان تحویل
        /// </summary>
        public POIncoterms DeliveryLocation { get; set; }

        /// <summary>
        /// نوع مرجع سفارش
        /// </summary>
        public PORefType PORefType { get; set; }

        /// <summary>
        /// وضعیت سفارش
        /// </summary>
        public POStatus POStatus { get; set; }

        [Required]
        public CurrencyType CurrencyType { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Tax { get; set; }

        /// نوع قرارداد 
        /// </summary>
        public PContractType PContractType { get; set; }

        /// <summary>
        /// شناسه تامین کننده
        /// </summary>
        public int SupplierId { get; set; }

        /// <summary>
        /// تاریخ تحویل
        /// </summary>
        [Required]
        public DateTime DateDelivery { get; set; }

        /// <summary>
        /// شناسه قرارداد
        /// </summary>
        public long PRContractId { get; set; }

        /// <summary>
        /// مبلغ سفارش
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal FinalTotalAmount { get; set; }

        public int ProductGroupId { get; set; }
        public bool IsPaymentDone { get; set; }
        public POShortageStatus ShortageStatus { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ProductGroupId))]
        public virtual ProductGroup ProductGroup { get; set; }

        [ForeignKey(nameof(PRContractId))]
        public virtual PRContract PRContract { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier Supplier { get; set; }

        [ForeignKey(nameof(BaseContractCode))]
        public virtual Contract Contract { get; set; }

        public virtual ICollection<POSubject> POSubjects { get; set; }


        public virtual ICollection<POTermsOfPayment> POTermsOfPayments { get; set; }

        public virtual ICollection<PAttachment> POAttachments { get; set; }

        public virtual ICollection<POStatusLog> POStatusLogs { get; set; }

        public virtual ICollection<POActivity> POActivities { get; set; }

        public virtual ICollection<Pack> Packs { get; set; }

        public virtual ICollection<Receipt> Receipts { get; set; }

        public virtual ICollection<ReceiptReject> ReceiptRejects { get; set; }

        public virtual ICollection<FinancialAccount> FinancialAccounts { get; set; }
        public virtual ICollection<PoProgress> POProgresses { get; set; }
    }
}