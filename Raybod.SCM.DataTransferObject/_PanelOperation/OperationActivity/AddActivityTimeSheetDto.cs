using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelOperation
{
    public class AddActivityTimeSheetDto
    {
        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        public string Duration { get; set; }

        public long? DateIssue { get; set; }

        public double? ProgressPercent { get; set; }
    }
}
