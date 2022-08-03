using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class AddWarehouseRequestConfirmationDto
    {

        [MaxLength(800)]
        public string Note { get; set; }
        public List<AddWarehouseRequestUserConfirmationDto> Users { get; set; }
    }
}
