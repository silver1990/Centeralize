using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class UserSeenScmAuditLog
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        public Guid SCMAuditLogId { get; set; }
        public bool IsSeen { get; set; }
        public DateTime? DateSeen { get; set; }

        public bool IsPin { get; set; }
        public DateTime? PinDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(SCMAuditLogId))]
        public SCMAuditLog SCMAuditLog { get; set; }
    }
}
