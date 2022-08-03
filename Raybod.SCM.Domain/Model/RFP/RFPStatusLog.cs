using Raybod.SCM.Domain.Enum;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class RFPStatusLog
    {
        [Key]
        public long Id { get; set; }

        public RFPLogStatus Status { get; set; }

        [Required]
        public DateTime DateIssued { get; set; }

        public long RFPId { get; set; }

        [ForeignKey(nameof(RFPId))]
        public RFP RFP { get; set; }
    }
}
