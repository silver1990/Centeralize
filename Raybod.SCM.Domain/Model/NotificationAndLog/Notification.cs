using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }

        public string BaseContratcCode { get; set; }

        public int PerformerUserId { get; set; }

        [Required]
        public DateTime DateCreate { get; set; } = DateTime.UtcNow;

        public DateTime? DateDone { get; set; }

        public NotifEvent NotifEvent { get; set; }

        [MaxLength(64)]
        public string FormCode { get; set; }

        [MaxLength(250)]
        public string Description { get; set; }

        [MaxLength(250)]
        public string Quantity { get; set; }

        [MaxLength(800)]
        public string Message { get; set; }

        [MaxLength(100)]
        public string KeyValue { get; set; }

        [MaxLength(100)]
        public string RootKeyValue { get; set; }

        [MaxLength(100)]
        public string RootKeyValue2 { get; set; }

        [MaxLength(250)]
        public string Temp { get; set; }

        public bool IsDone { get; set; }
        
        [ForeignKey(nameof(PerformerUserId))]
        public User PerformerUser { get; set; }

        public virtual ICollection<UserNotification> UserNotifications { get; set; }
    }
}
