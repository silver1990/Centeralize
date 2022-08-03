using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RFPSupplier : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// شناسه RFP
        /// </summary>
        public long RFPId { get; set; }

        /// <summary>
        /// شناسه تامین کننده
        /// </summary>
        public int SupplierId { get; set; }

        /// <summary>
        /// امتیاز ارزیابی فنی
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TBEScore { get; set; }

        /// <summary>
        /// امتیاز ارزیابی بازرگانی
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CBEScore { get; set; }

        /// <summary>
        /// برنده مزایده شده ؟
        /// </summary>
        public bool IsWinner { get; set; } = false;

        /// <summary>
        /// فعال هست ؟
        /// </summary>
        public bool IsActive { get; set; } = false;

        [MaxLength(800)]
        public string TBENote { get; set; }

        [MaxLength(800)]
        public string CBENote { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier Supplier { get; set; }

        [ForeignKey(nameof(RFPId))]
        public virtual RFP RFP { get; set; }

        public virtual ICollection<RFPSupplierProposal> RFPSupplierProposals { get; set; }
        public virtual ICollection<RFPProForma> RFPProFormas { get; set; }

        public virtual ICollection<RFPComment> RFPComments { get; set; }
    }
}
