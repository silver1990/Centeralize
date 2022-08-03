using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PurchaseRequest
{
    public class AddPurchaseRequestItemDto
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public decimal Quntity { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateStart { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateEnd { get; set; }

    }

    public class AddPurchaseRequestItemsDto
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public decimal Quntity { get; set; }



    }
    public class AddPurchaseRequestItemsSysAdminDto
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public decimal RequiredQuantity { get; set; }



    }
}
