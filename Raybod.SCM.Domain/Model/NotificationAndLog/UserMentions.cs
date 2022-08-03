using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Raybod.SCM.Domain.Model
{
    public class UserMentions
    {
        [Key]
        public Guid Id { get; set; }

        public string BaseContratcCode { get; set; }

        public int PerformerUserId { get; set; }

        [Required]
        public DateTime DateCreate { get; set; } = DateTime.UtcNow;

        public MentionEvent MentionEvent { get; set; }

        [MaxLength(64)]
        public string FormCode { get; set; }

        [MaxLength(250)]
        public string Description { get; set; }

        [MaxLength(800)]
        public string Message { get; set; }

        [MaxLength(100)]
        public string KeyValue { get; set; }

        [MaxLength(100)]
        public string RootKeyValue { get; set; }

        [MaxLength(100)]
        public string RootKeyValue2 { get; set; }
        public string Temp { get; set; }


        public bool IsSeen { get; set; }

        public DateTime? SeenDate { get; set; }

        public bool IsPin { get; set; }

        public DateTime? PinDate { get; set; }

        [ForeignKey(nameof(PerformerUserId))]
        public User PerformerUser { get; set; }

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
