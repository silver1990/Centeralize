using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RFPItems : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public long RFPId { get; set; }

        public int ProductId { get; set; }

        public bool IsActive { get; set; }

        public long? PurchaseRequestItemId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RemainedStock { get; set; }

        [Required]
        public DateTime DateStart { get; set; }

        [Required]
        public DateTime DateEnd { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }

        [ForeignKey(nameof(RFPId))]
        public virtual RFP RFP { get; set; }

        [ForeignKey(nameof(PurchaseRequestItemId))]
        public virtual PurchaseRequestItem PurchaseRequestItem { get; set; }

        public virtual ICollection<PRContractSubject> PRContractSubjects { get; set; }
    }
}
