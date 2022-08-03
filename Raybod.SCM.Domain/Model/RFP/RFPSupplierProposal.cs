using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RFPSupplierProposal : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public long RFPSupplierId { get; set; }

        public long RFPInqueryId { get; set; }

        public bool IsAnswered { get; set; }

        public bool IsEvaluated { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal EvaluationScore { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(RFPInqueryId))]
        public RFPInquery RFPInquery { get; set; }

        [ForeignKey(nameof(RFPSupplierId))]
        public RFPSupplier RFPSupplier { get; set; }

        public ICollection<RFPAttachment> RFPSupplierInqueryAttachments { get; set; }
    }
}
