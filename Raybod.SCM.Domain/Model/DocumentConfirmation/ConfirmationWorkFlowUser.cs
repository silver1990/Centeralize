using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class ConfirmationWorkFlowUser
    {
        [Key]
        public long ConfirmationWorkFlowUserId { get; set; }

        public long ConfirmationWorkFlowId { get; set; }

        public int UserId { get; set; }

        public int OrderNumber { get; set; }

        [MaxLength(800)]
        public string Note { get; set; }

        public bool IsBallInCourt { get; set; }

        public bool IsAccept { get; set; }

        public DateTime? DateStart { get; set; }

        public DateTime? DateEnd { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        [ForeignKey(nameof(ConfirmationWorkFlowId))]
        public virtual ConfirmationWorkFlow ConfirmationWorkFlow { get; set; }

    }
}
