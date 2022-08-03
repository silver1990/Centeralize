using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class Logistic : BaseAuditEntity
    {
        [Key]
        public long LogisticId { get; set; }

        public long PackId { get; set; }

        public LogisticStep Step { get; set; }

        public DateTime? DateStart { get; set; }

        public DateTime? DateEnd { get; set; }

        public LogisticStatus LogisticStatus { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey(nameof(PackId))]
        public virtual Pack Pack { get; set; }

        public virtual ICollection<PAttachment> Attachments { get; set; }


    }
}
