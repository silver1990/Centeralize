using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.DataTransferObject._panelPurchase.PurchaseRequestConfirmation
{
    public class AddPurchaseRequestConfirmationAnswerDto
    {
        [MaxLength(800)]
        public string Note { get; set; }

        public bool IsAccept { get; set; }
    }
}
