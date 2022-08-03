using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class AddPOSubjectDto
    {
        public long POId { get; set; }

        public long MrpItemId { get; set; }

        [Display(Name = "محصول")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public int ProductId { get; set; }

        [Display(Name = "تیراژ")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public decimal Quantity { get; set; }
    }
}
