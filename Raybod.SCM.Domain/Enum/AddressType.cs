using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    // no db
    public enum AddressType
    {
        None = 0,

        [Display(Name = "درب کارخانه تامین کننده")]
        SupplierLocation = 1,

        [Display(Name = "کمرک مبدا")]
        OriginPort = 2,

        [Display(Name = "کمرک مقصد")]
        DestinationPort = 3,

        [Display(Name = "درب کارخانه خریدار")]
        CompanyLocation = 4,
    }
}
