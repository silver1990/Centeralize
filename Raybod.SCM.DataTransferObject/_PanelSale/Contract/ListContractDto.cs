using Raybod.SCM.DataTransferObject.Consultant;
using Raybod.SCM.DataTransferObject.Customer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class ListContractDto : BaseContractDto
    {      

        [Display(Name = "مبلغ به حروف")]
        public string CostInLetters { get; set; }
        
        [Display(Name = "نام مشتری")]
        public string CustomerName { get; set; }
        [Display(Name = "نام مشاور")]
        public string ConsultantName { get; set; }

        [Display(Name = "تاریخ تنظیم")]
        public long? CreatedDate { get; set; }

        [Display(Name = "مدت قرارداد")]
        public int ContractDuration { get; set; }

        [Display(Name = "تاریخ قرار داد")]
        [Required(ErrorMessage = "فیلد اجباریست")]
        public long? DateIssued { get; set; }

        [Display(Name = "تاریخ پایان قرارداد")]
        public long? DateEnd { get; set; }

        [Display(Name = "تاریخ موثر شدن")]
        public long? DateEffective { get; set; }


        
        public BaseCustomerDto CustomerInfo { get; set; }
        public BaseConsultantDto ConsultantInfo { get; set; }
        
        public UserAuditLogDto UserAudit { get; set; }
        
    }
}
