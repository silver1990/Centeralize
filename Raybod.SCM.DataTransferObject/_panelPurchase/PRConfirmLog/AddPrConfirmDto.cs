using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class AddPrConfirmDto
    {
        public bool IsConfirm { get; set; }

        [MaxLength(800)]
        public string Note { get; set; }
    }
}
