using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class AddWarehouseRequestUserConfirmationDto
    {
        public int UserId { get; set; }

        public int OrderNumber { get; set; }
    }
}
