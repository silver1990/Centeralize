using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class SCMAuditLog
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string BaseContractCode { get; set; }

        public int? PerformerUserId { get; set; }

        [Required]
        public DateTime DateCreate { get; set; } = DateTime.UtcNow;

        public NotifEvent NotifEvent { get; set; }

        [MaxLength(800)]
        public string Message { get; set; } = "یه خبری شده";

        [MaxLength(100)]
        public string KeyValue { get; set; }

        [MaxLength(100)]
        public string RootKeyValue { get; set; }

        [MaxLength(100)]
        public string RootKeyValue2 { get; set; }

        [MaxLength(64)]
        public string FormCode { get; set; }

        [MaxLength(250)]
        public string Description { get; set; }

        [MaxLength(250)]
        public string Quantity { get; set; }

        [MaxLength(250)]
        public string Temp { get; set; }
        public string EventNumber { get; set; }


        public int? ProductGroupId { get; set; }

        public int? DocumentGroupId { get; set; }
        public int? OperationGroupId { get; set; }
        public virtual ICollection<UserSeenScmAuditLog> UserSCMAuditLogs { get; set; }

        public virtual ICollection<LogUserReceiver> LogUserReceivers { get; set; }
        public virtual ICollection<UserPinAuditLog> UserPinAuditLogs { get; set; }

        [ForeignKey(nameof(PerformerUserId))]
        public User PerformerUser { get; set; }

        [ForeignKey(nameof(BaseContractCode))]
        public Contract Contract { get; set; }

        [ForeignKey(nameof(DocumentGroupId))]
        public DocumentGroup DocumentGroup { get; set; }

        [ForeignKey(nameof(ProductGroupId))]
        public ProductGroup ProductGroup { get; set; }

    }
}
