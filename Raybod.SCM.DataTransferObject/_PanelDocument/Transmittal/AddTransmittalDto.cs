using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Transmittal
{
    public class AddTransmittalDto
    {
        public TransmittalType TransmittalType { get; set; }

        [MaxLength(800)]
        public string Description { get; set; }

        public int? SupplierId { get; set; }
        public int? ConsultantId { get; set; }

        [MaxLength(200)]
        public string FullName { get; set; }

        [MaxLength(300)]
        public string Email { get; set; }

        public List<AddTransmittalRevisionDto> Revisions { get; set; }
    }
}
