using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PO.POInspection
{
    public class AddPOInspectionDto
    {
        [MaxLength(800)]
        [Required]
        public string Description { get; set; }
        public long? DueDate { get; set; }
        public int InspectorId { get; set; }

    }
}
