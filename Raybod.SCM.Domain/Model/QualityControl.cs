using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.Domain.Model
{
    public class QualityControl : BaseEntity
    {
        [Key] 
        public long Id { get; set; }

        public long? ReceiptId { get; set; }

        public long? PackId { get; set; }

        [MaxLength(800)]
        public string Note { get; set; }

        public QCResult QCResult { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ReceiptId))]
        public Receipt Receipt { get; set; }
        
        [ForeignKey(nameof(PackId))]
        public Pack Pack { get; set; }

        public virtual ICollection<PAttachment> QCAttachments { get; set; }
    }
}