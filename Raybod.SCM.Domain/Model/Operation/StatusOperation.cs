using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class StatusOperation:BaseEntity
    {
        public long Id { get; set; }

        public Guid OperationId { get; set; }

        public OperationStatus OperationStatus { get; set; }


        [ForeignKey(nameof(OperationId))]
        public virtual Operation Operation { get; set; }
    }
}
