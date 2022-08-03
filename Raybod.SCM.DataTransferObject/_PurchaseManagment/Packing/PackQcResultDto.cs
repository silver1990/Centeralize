using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Packing
{
    public class PackQcResultDto
    {
        public PackStatus PackStatus { get; set; }
        public string PackCode { get; set; }
        public PackingQualityControlInfodto QualityControlInfo { get; set; }
    }
}
