using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class WarehouseOutputRequestWorkFlow:BaseEntity
    {
        [Key]
        public long RequestWorkFlowId { get; set; }
        public long RequestId { get; set; }

        [MaxLength(800)]
        public string ConfirmNote { get; set; }


        public ConfirmationWorkFlowStatus Status { get; set; }



        [ForeignKey(nameof(RequestId))]
        public virtual WarehouseOutputRequest WarehouseOutputRequest { get; set; }


        public virtual ICollection<WarehouseOutputRequestWorkFlowUser> WarehouseOutputRequestWorkFlowUsers { get; set; }
    }
}
