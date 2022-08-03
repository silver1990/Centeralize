using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class OperationActivityTimesheet:BaseEntity
    {
        [Key]
        public long ActivityTimesheetId { get; set; }

        public long OperationActivityId { get; set; }

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

        [ForeignKey(nameof(OperationActivityId))]
        public virtual OperationActivity OperationActivity { get; set; }
    }
}
