using System;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PO.POActivity
{
    public class AddActivityTimeSheetDto
    {

        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        public string Duration { get; set; }

        [Required]
        public long DateIssue { get; set; }
        [Required]
        public double ProgressPercent { get; set; }

    }
}
