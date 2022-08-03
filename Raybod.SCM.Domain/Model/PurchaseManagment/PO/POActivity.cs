using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class POActivity
    {
        [Key]
        public long POActivityId { get; set; }

        public long POId { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public POActivityStatus ActivityStatus { get; set; }

        public DateTime? DateEnd { get; set; }

        public int ActivityOwnerId { get; set; }

        public bool IsDeleted { get; set; }

        public double Duration { get; set; }
        public double Weight { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ActivityOwnerId))]
        public virtual User ActivityOwner { get; set; }

        [ForeignKey(nameof(POId))]
        public virtual PO PO { get; set; }

        public virtual ICollection<POActivityTimesheet> ActivityTimesheets { get; set; }

    }
}
