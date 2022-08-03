using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class TransmittalRevision
    {
        public int Id { get; set; }

        public long TransmittalId { get; set; }

        public long DocumentRevisionId { get; set; }

        public POI POI { get; set; }

        [ForeignKey(nameof(TransmittalId))]
        public virtual Transmittal Transmittal { get; set; }

        [ForeignKey(nameof(DocumentRevisionId))]
        public virtual DocumentRevision DocumentRevision { get; set; }
    }
}
