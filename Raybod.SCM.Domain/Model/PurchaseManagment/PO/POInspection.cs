using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class POInspection:BaseEntity
    {
        [Key]
        public long POInspectionId { get; set; }

        [MaxLength(2048)]
        public string Description { get; set; }
        public int InspectorId { get; set; }
        public DateTime? DueDate { get; set; }
        public InspectionResult Result { get; set; }

        [MaxLength(2048)]
        public string ResultNote { get; set; }
        public long POId { get; set; }

        [ForeignKey(nameof(InspectorId))]
        public virtual User Inspector { get; set; }
        [ForeignKey(nameof(POId))]
        public virtual PO PO { get; set; }
        public virtual ICollection<PAttachment> Attachments { get; set; }
    }
}
