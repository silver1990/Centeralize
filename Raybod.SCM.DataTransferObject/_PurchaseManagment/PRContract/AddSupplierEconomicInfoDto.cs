using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class AddSupplierEconomicInfoDto
    {

        [MaxLength(150, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string EconomicCode { get; set; }

        [MaxLength(150, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string NationalId { get; set; }

    }
}
