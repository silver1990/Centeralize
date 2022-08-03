using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RevisionActivity
    {
        [Key]
        public long RevisionActivityId { get; set; }

        public long RevisionId { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public RevisionActivityStatus RevisionActivityStatus { get; set; }

        public DateTime? DateEnd { get; set; }

        public int ActivityOwnerId { get; set; }

        public bool IsDeleted { get; set; }

        public double Duration { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ActivityOwnerId))]
        public virtual User ActivityOwner { get; set; }

        [ForeignKey(nameof(RevisionId))]
        public virtual DocumentRevision DocumentRevision { get; set; }

        public virtual ICollection<RevisionActivityTimesheet> RevisionActivityTimesheets { get; set; }

    }
}
