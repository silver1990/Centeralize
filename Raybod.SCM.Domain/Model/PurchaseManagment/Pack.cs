using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class Pack : BaseEntity
    {
        [Key]
        public long PackId { get; set; }

        public long POId { get; set; }

        [MaxLength(64)]
        public string PackCode { get; set; }

        public PackStatus PackStatus { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(POId))]
        public PO PO { get; set; }

        public virtual Receipt Receipt { get; set; }

        public virtual ICollection<PackSubject> PackSubjects { get; set; }

        public virtual ICollection<QualityControl> QualityControls { get; set; }

        public virtual ICollection<Logistic> Logistics { get; set; }

        public virtual ICollection<PAttachment> PackAttachments { get; set; }

    }
}
