using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class POActivityTimesheet : BaseEntity
    {
        [Key]
        public long ActivityTimesheetId { get; set; }

        public long POActivityId { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        public TimeSpan Duration { get; set; }

        [Required]
        public DateTime DateIssue { get; set; }

        [Required]
        public double ProgressPercent { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(POActivityId))]
        public virtual POActivity POActivity { get; set; }

    }
}
