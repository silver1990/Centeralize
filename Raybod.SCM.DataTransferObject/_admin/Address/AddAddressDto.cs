using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Address
{
    public class AddAddressDto
    {
        [MaxLength(250)] public string DeliveryLocation { get; set; }

        [Display(Name = "کد پستی")]
        [MaxLength(10, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string PostalCode { get; set; }

        [MaxLength(14, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string Phone { get; set; }

        public AddressType AddressType { get; set; }

        [Display(Name = "کشور")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        [MaxLength(150, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string Country { get; set; }

        [Display(Name = "استان")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        [MaxLength(150, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string Province { get; set; }

        [Display(Name = "شهر")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        [MaxLength(150, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string City { get; set; }

        [Display(Name = "آدرس")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        [StringLength(800, MinimumLength = 3, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string StreetAddress { get; set; }

        [Display(Name = "شناسه شرکت")] public int? CompanyId { get; set; }
        [Display(Name = "شناسه تامین کننده")] public int? SupplierId { get; set; }

        [Display(Name = "شناسه مشتری")] public int? CustomerId { get; set; }
    }
}