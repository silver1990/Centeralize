using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.Domain.Model
{
    public class PRContract : BaseAuditEntity
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// شناسه پیشنهاد درخواست خرید
        /// </summary>
        public int ProductGroupId { get; set; }

        /// <summary>
        /// کد قرارداد
        /// </summary>
        [MaxLength(64)]
        public string PRContractCode { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string BaseContractCode { get; set; }

        /// <summary>
        /// شناسه تامین کننده
        /// </summary>
        public int SupplierId { get; set; }

        /// <summary>
        /// مکان تحویل
        /// </summary>
        public POIncoterms DeliveryLocation { get; set; }

        /// <summary>
        /// نوع قرارداد
        /// </summary>
        public PContractType PContractType { get; set; }

        /// <summary>
        /// واحد پول قرارداد
        /// </summary>
        public CurrencyType CurrencyType { get; set; }

        /// <summary>
        /// تاریخ ثبت قرارداد
        /// </summary>
        [Required]
        public DateTime DateIssued { get; set; }

        /// <summary>
        /// تاریخ پایان قرارداد
        /// </summary>
        [Required]
        public DateTime DateEnd { get; set; }

        /// <summary>
        /// الریخ شروع قرارداد
        /// </summary>
        [Required]
        public int ContractDuration { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Tax { get; set; }

        /// <summary>
        /// مبلغ قرارداد
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// مبلغ نهایی قرارداد
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal FinalTotalAmount { get; set; }

        /// <summary>
        /// وضعیت قرارداد
        /// </summary>
        public PRContractStatus PRContractStatus { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ProductGroupId))]
        public virtual ProductGroup ProductGroup { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier Supplier { get; set; }

        public virtual ICollection<POTermsOfPayment> TermsOfPayments { get; set; }


        public virtual ICollection<PRContractSubject> PRContractSubjects { get; set; }

        public virtual ICollection<PAttachment> PRContractAttachments { get; set; }
        public virtual ICollection<PrContractConfirmationWorkFlow> PrContractConfirmationWorkFlows { get; set; }

        public virtual ICollection<PO> POs { get; set; }
    }
}