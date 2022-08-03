using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RFP : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public int ProductGroupId { get; set; }

        [Required]
        [MaxLength(64)]
        public string RFPNumber { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }

        [Required]
        public DateTime DateDue { get; set; }

        public RFPType RFPType { get; set; }

        public RFPStatus Status { get; set; }

        [MaxLength(800)]
        public string Note { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ContractCode))]
        public virtual Contract Contract { get; set; }

        [ForeignKey(nameof(ProductGroupId))]
        public virtual ProductGroup ProductGroup { get; set; }

        public virtual ICollection<RFPItems> RFPItems { get; set; }

        public virtual ICollection<RFPInquery> RFPInqueries { get; set; }

        public virtual ICollection<RFPAttachment> RFPAttachments { get; set; }

        public virtual ICollection<RFPSupplier> RFPSuppliers { get; set; }

        public virtual ICollection<RFPStatusLog> RFPStatusLogs { get; set; }

        //public virtual ICollection<RFPEvaluation> RFPEvaluations { get; set; }
    }
}
