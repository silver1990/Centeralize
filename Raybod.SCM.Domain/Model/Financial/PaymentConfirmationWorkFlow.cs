using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class PaymentConfirmationWorkFlow:BaseEntity
    {
        [Key]
        public long PaymentConfirmWorkFlowId { get; set; }

        public long? PaymentId { get; set; }

        [MaxLength(800)]
        public string ConfirmNote { get; set; }


        public ConfirmationWorkFlowStatus Status { get; set; }



        [ForeignKey(nameof(PaymentId))]
        public virtual Payment Payment { get; set; }

        public virtual ICollection<PaymentAttachment> PaymentConfirmationAttachments { get; set; }

        public virtual ICollection<PaymentConfirmationWorkFlowUser> PaymentConfirmationWorkFlowUsers { get; set; }
    }
}
