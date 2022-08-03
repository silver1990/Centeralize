using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class WarehouseOutputRequestWorkFlowUser
    {
        [Key]
        public long WarehouseOutputRequestWorkFlowUserId { get; set; }

        public long RequestWorkFlowId { get; set; }

        public int UserId { get; set; }

        public int OrderNumber { get; set; }

        [MaxLength(800)]
        public string Note { get; set; }

        public bool IsBallInCourt { get; set; }

        public bool IsAccept { get; set; }

        public DateTime? DateStart { get; set; }

        public DateTime? DateEnd { get; set; }



        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        [ForeignKey(nameof(RequestWorkFlowId))]
        public virtual WarehouseOutputRequestWorkFlow GetWarehouseOutputRequestWorkFlow { get; set; }
    }
}
