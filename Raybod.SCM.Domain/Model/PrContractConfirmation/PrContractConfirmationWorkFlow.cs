using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class PrContractConfirmationWorkFlow:BaseEntity
    {
        [Key]
        public long PrContractConfirmWorkFlowId { get; set; }

        public long? PrContractId { get; set; }

        [MaxLength(800)]
        public string ConfirmNote { get; set; }


        public ConfirmationWorkFlowStatus Status { get; set; }



        [ForeignKey(nameof(PrContractId))]
        public virtual PRContract PRContract { get; set; }

        public virtual ICollection<PAttachment> PrContractConfirmationAttachments { get; set; }

        public virtual ICollection<PrContractConfirmationWorkFlowUser> PrContractConfirmationWorkFlowUsers{ get; set; }
    }
}
