using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Supplier.Address
{
    public class BaseSupplierAddressDto
    {
        [Display(Name ="نام مکان")]
        [MaxLength(250, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string DeliveryLocation { get; set; }

        [Display(Name = "کد پستی")]
        [MaxLength(10, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string PostalCode { get; set; }

        [Display(Name = "شماره تلفن")]
        [MaxLength(14, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string Phone { get; set; }

        [Display(Name = "کشور")]
        [Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(150, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string Country { get; set; }

        [Display(Name = "استان")]
        [Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(150, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string Province { get; set; }

        [Display(Name = "شهر")]
        [Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(150, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string City { get; set; }

        [Display(Name = "آدرس")]
        [Required(ErrorMessage = "الزامی می باشد")]
        [StringLength(800, MinimumLength = 3, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string StreetAddress { get; set; }

        [Display(Name = "شناسه تامین کننده")]
        public int SupplierId { get; set; }
    }
}
