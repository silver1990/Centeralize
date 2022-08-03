using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class AddContractDto : BaseContractDto
    {
        [Display(Name = "تاریخ قرار داد")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public long DateIssued { get; set; }

        [Display(Name = "تاریخ پایان قرارداد")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public long DateEnd { get; set; }

        [Display(Name = "تاریخ موثر شدن")]
        [Required(ErrorMessage = "فیلد الزامیست")]
        public long DateEffective { get; set; }

        [Display(Name = "مدت قرارداد")]
        [Required(ErrorMessage = "فیلد الزامیست")]
        public int ContractDuration { get; set; }

        public List<AddContractSubjectsDto> ContractSubjects { get; set; }

        public List<AddAttachmentDto> Attachment { get; set; }

        public List<AddContractFormConfigDto> FormConfig { get; set; }

    }
    public class InsertContractDto
    {
        [Display(Name = "کد قرارداد")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        [MaxLength(64, ErrorMessage = "حداکثر مقدار برای فیلد {1} می باشد.")]
        public string ContractCode { get; set; }

        [Display(Name = "توضیحات")]
        public string Description { get; set; }
        public List<AddContractFormConfigDto> FormConfig { get; set; }
        public List<string> Services { get; set; }
        public InsertContractDto()
        {
            FormConfig = new List<AddContractFormConfigDto>();
            Services = new List<string>();
        }
    }
}
