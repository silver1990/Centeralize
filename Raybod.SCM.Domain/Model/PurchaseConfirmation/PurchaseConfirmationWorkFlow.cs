using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class PurchaseConfirmationWorkFlow:BaseEntity
    {
        [Key]
        public long PurchaseRequestConfirmWorkFlowId { get; set; }

        public long? PurchaseRequestId { get; set; }

        [MaxLength(800)]
        public string ConfirmNote { get; set; }


        public ConfirmationWorkFlowStatus Status { get; set; }



        [ForeignKey(nameof(PurchaseRequestId))]
        public virtual PurchaseRequest PurchaseRequest { get; set; }

        public virtual ICollection<PAttachment> PurchaseConfirmationAttachments { get; set; }

        public virtual ICollection<PurchaseConfirmationWorkFlowUser> PurchaseConfirmationWorkFlowUsers { get; set; }
    }
}
