using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject._PanelOperation.OperationGroup
{
    public class AddOperationGroupDto
    {
        [Required]
        [MaxLength(64)]
        public string OperationGroupCode { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }
    }
}
