using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class BaseContractDto
    {
        [Display(Name = "کد قرارداد")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        [MaxLength(64, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string ContractCode { get; set; }

        [Display(Name = "شماره قرارداد")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public string ContractNumber { get; set; }


        [Display(Name = "کد قرارداد")]
        [StringLength(800, MinimumLength = 3, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string Description { get; set; }

        [Display(Name = "کد قرارداد اصلی")]
        public string ParentContractCode { get; set; }

        [Display(Name = "توضیحات")]
        public string Details { get; set; }

        [Display(Name = "مبلغ")]
        public decimal? Cost { get; set; }

        [Display(Name = "وضعیت قرارداد")]
        public ContractStatus ContractStatus { get; set; }

        [Display(Name = "نوع قرارداد")]
        public ContractType ContractType { get; set; }

        [Display(Name = "مشتری")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public int CustomerId { get; set; }

        [Display(Name = "مشاور")]
       
        public int? ConsultantId { get; set; }

    }
}
