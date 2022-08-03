using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class EditContractDto 
    {
        [Display(Name = "کد قرارداد")]
        [Required(ErrorMessage = "فیلد اجباریست")]        
        public string ContractCode { get; set; }

        [Display(Name = "شماره قرارداد")]
        [StringLength(250,MinimumLength =1, ErrorMessage = "فیلد اجباریست")]
        public string ContractNumber { get; set; }

        [Display(Name = "کد قرارداد")]
        [StringLength(800, MinimumLength = 3, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string Description { get; set; }

        [Display(Name = "وضعیت قرارداد")]
        public ContractStatus ContractStatus { get; set; }

        [Display(Name = "تاریخ قرار داد")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public long DateIssued { get; set; }

        [Display(Name = "تاریخ پایان قرارداد")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public long DateEnd { get; set; }

        [Display(Name = "تاریخ موثر شدن")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public long DateEffective { get; set; }

        [Display(Name = "مدت قرارداد")]
        public int ContractDuration { get; set; }
    }
}
