using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class UserPinAuditLog
    {
        [Key]
        public int Id { get; set; }
        public Guid EventId { get; set; }
        public int UserId { get; set; }
        public bool IsPin { get; set; }
        public DateTime? PinDate { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(EventId))]
        public SCMAuditLog SCMAuditLog { get; set; }
    }
}
