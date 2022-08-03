using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class PurchaseRequestItem : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public int ProductId { get; set; }

        public long PurchaseRequestId { get; set; }

        public PRItemStatus PRItemStatus { get; set; }
        public long? MrpItemId { get; set; }
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quntity { get; set; }

        [Required]
        public DateTime DateStart { get; set; }

        public DateTime DateEnd { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }

        [ForeignKey(nameof(MrpItemId))]
        public virtual MrpItem MrpItem { get; set; }

        [ForeignKey(nameof(PurchaseRequestId))]
        public virtual PurchaseRequest PurchaseRequest { get; set; }

        public virtual ICollection<RFPItems> RFPItems { get; set; }

    }
}
