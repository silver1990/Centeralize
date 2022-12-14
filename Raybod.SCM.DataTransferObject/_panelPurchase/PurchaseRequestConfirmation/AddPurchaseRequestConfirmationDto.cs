using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject._panelPurchase.PurchaseRequestConfirmation
{
    public class AddPurchaseRequestConfirmationDto
    {

        [MaxLength(800)]
        public string Note { get; set; }
        public List<AddPurchaseRequestUserConfirmationDto> Users { get; set; }
    }
}
