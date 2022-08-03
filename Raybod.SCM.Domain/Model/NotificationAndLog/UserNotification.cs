using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class UserNotification
    {
        public long UserNotificationsId { get; set; }

        public int UserId { get; set; }

        public bool IsSeen { get; set; }
        public bool IsPin { get; set; }

        public DateTime? DateSeen { get; set; }
        public DateTime? PinDate { get; set; }

        public bool IsUserSetTaskDone { get; set; }

        public Guid NotificationId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(NotificationId))]
        public Notification Notification { get; set; }
    }
}
