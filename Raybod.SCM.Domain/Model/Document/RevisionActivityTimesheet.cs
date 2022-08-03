using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RevisionActivityTimesheet : BaseEntity
    {
        [Key]
        public long ActivityTimesheetId { get; set; }

        public long RevisionActivityId { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        public TimeSpan Duration { get; set; }

        [Required]
        public DateTime DateIssue { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(RevisionActivityId))]
        public virtual RevisionActivity RevisionActivity { get; set; }

        public virtual ICollection<RevisionAttachment> ActivityTimesheetAttachments { get; set; }
    }
}
