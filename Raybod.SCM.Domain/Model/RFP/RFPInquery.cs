using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RFPInquery : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public long RFPId { get; set; }

        public RFPInqueryType RFPInqueryType { get; set; }

        public DefaultInquery DefaultInquery { get; set; }

        /// <summary>
        /// شرح استعلام
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Weight { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual ICollection<RFPAttachment> RFPInqueryAttachments { get; set; }

        [ForeignKey(nameof(RFPId))]
        public RFP RFP { get; set; }

        public ICollection<RFPSupplierProposal> RFPSupplierProposal { get; set; }
        public ICollection<RFPCommentInquery> RFPCommentInqueries { get; set; }

    }
}
