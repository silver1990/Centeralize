using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class TeamWorkUserOperationGroup
    {
        public int Id { get; set; }

        public int OperationGroupId { get; set; }

        public int TeamWorkUserId { get; set; }

        public int UserId { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }

        [ForeignKey(nameof(TeamWorkUserId))]
        public virtual TeamWorkUser TeamWorkUser { get; set; }

        [ForeignKey(nameof(OperationGroupId))]
        public virtual OperationGroup OperationGroup { get; set; }
    }
}
