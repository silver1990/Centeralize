using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelDocument.Area
{
    public class AreaAddDTO
    {
        [MaxLength(200)]
        public string AreaTitle { get; set; }
    }
}
