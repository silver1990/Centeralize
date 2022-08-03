using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Product
{
    public class DuplicateOptionDto
    {
        public MaterialType Value { get; set; }
        public string Label { get; set; }
    }
}
