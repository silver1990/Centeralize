using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class OperationActivity:BaseEntity
    {
        [Key]
        public long OperationActivityId { get; set; }

        public Guid OperationId { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public OperationActivityStatus OperationActivityStatus { get; set; }

        public DateTime? DateEnd { get; set; }

        public int ActivityOwnerId { get; set; }


        public double Duration { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(ActivityOwnerId))]
        public virtual User ActivityOwner { get; set; }

        [ForeignKey(nameof(OperationId))]
        public virtual Operation Operation { get; set; }

        public double Weight { get; set; }
        public virtual ICollection<OperationActivityTimesheet> OperationActivityTimesheets { get; set; }
    }
}
