using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class AddPOByMrpDto
    {
        public long PRContractId { get; set; }

        public string PRContractCode { get; set; }

        public int SupplierId { get; set; }

        public string SupplierName { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public long PRContractSubjectId { get; set; }

        [Display(Name = "محصول")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public int ProductId { get; set; }

        [Display(Name = "تیراژ")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// حداکثر سقف قابل سفارش
        /// </summary>
        public decimal RemainedStock { get; set; }

        /// <summary>
        /// میزان سفارش
        /// </summary>
        public decimal OrderAmount { get; set; }


    }
}