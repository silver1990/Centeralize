using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Transmittal
{
    public class TransmittalCompanyListDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public TransmittalType Type { get; set; }
    }
}
