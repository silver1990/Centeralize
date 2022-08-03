using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Contract
{
    public class ContractMiniInfoDto
    {

        [Display(Name = "کد قرارداد")]
        public string ContractCode { get; set; }

        [Display(Name = "شماره قرارداد")]
        public string ContractNumber { get; set; }

        [Display(Name = "کد قرارداد")]
        public string Description { get; set; }

        [Display(Name = "نوع قرارداد")]
        public ContractType ContractType { get; set; }

        public int CustomerId { get; set; }

        [Display(Name = "نام مشتری")]
        public string CustomerName { get; set; }

        public List<ContractSubjectMiniInfoDto> ContractSubjects { get; set; }

    }
}
