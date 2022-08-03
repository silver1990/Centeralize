using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Payment
{
    public class AddPaymentUserConfirmationDto
    {
        public int UserId { get; set; }

        public int OrderNumber { get; set; }
    }
}
