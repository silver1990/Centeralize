using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class BaseMrpDto
    {

        [Display(Name = "کد قرارداد")]
        public string ContractCode { get; set; }

        [Display(Name = "شرح")]
        [Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(300, ErrorMessage = "حداکثر مقدار فیلد {1} می باشد")]
        public string Description { get; set; }

        [Display(Name = "وضعیت")]
        public MrpStatus MrpStatus { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        [MaxLength(64)]
        public string MrpNumber { get; set; }
    }
}
