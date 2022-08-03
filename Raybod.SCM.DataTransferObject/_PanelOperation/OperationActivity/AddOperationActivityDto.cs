using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelOperation
{
   public class AddOperationActivityDto
    {
        [MaxLength(800)]
        [Required]
        public string Description { get; set; }

        public long? DateEnd { get; set; }

        public int ActivityOwnerId { get; set; }
        public double Weight { get; set; }
    }
}
