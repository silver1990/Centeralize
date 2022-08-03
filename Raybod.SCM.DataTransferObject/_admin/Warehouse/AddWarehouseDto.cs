using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Warehouse
{
    public class AddWarehouseDto
    {
        [Required(ErrorMessage = "الزامی می باشد")]
        [Display(Name = "نام")]
        [MaxLength(250, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string Name { get; set; }

        [Display(Name = "کد انبار")]
        [MaxLength(50, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string WarehouseCode { get; set; }

        [Display(Name = "شماره تلفن")]
        [MaxLength(20, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string Phone { get; set; }

        [Display(Name = "آدرس")]
        [Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(300, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string Address { get; set; }

    }
}
