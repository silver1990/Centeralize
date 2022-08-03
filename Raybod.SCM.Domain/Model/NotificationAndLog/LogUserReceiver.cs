using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class LogUserReceiver
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        public Guid SCMAuditLogId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(SCMAuditLogId))]
        public SCMAuditLog SCMAuditLog { get; set; }
    }
}
