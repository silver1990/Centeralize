using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{ 
    public class OperationProgress:BaseEntity
    {
        public long Id { get; set; }

        public Guid OperationId { get; set; }

        public double ProgressPercent { get; set; }


        [ForeignKey(nameof(OperationId))]
        public virtual Operation Operation { get; set; }
    }
}
