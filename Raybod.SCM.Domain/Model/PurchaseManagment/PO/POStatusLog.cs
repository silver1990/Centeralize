using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.Domain.Model
{
    public class POStatusLog : BaseAuditEntity
    {
        [Key] public long Id { get; set; }

        public POStatus BeforeStatus { get; set; }

        public POStatus Status { get; set; }

        public bool IsDone { get; set; } = false;

        public long POId { get; set; }

        [ForeignKey(nameof(POId))] public PO PO { get; set; }
    }
}