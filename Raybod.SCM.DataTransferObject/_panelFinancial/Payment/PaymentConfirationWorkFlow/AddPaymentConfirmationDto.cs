using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Payment
{
    public class AddPaymentConfirmationDto
    {
        [MaxLength(800)]
        public string Note { get; set; }
        public List<AddPaymentUserConfirmationDto> Users { get; set; }

    }
}
