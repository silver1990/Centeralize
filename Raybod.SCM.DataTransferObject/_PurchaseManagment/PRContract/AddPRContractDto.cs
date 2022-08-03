using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class AddPRContractDto 
    {


        public ContractTimeTable ContractTimeTable { get; set; }
        public int ProductGroupId { get; set; }

        public int SupplierId { get; set; }
        public ContractType ContractType { get; set; }
        public PaymentSteps PaymentSteps { get; set; }

        public List<AddPRContractSubjectDto> PRContractSubjects { get; set; }


        public List<AddAttachmentDto> PRCAttachments { get; set; }

        public AddPrContractConfirmationDto WorkFlow { get; set; }

    }
    public class ContractTimeTable
    {
        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateIssued { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public long DateEnd { get; set; }

        [Required(ErrorMessage = "الزامی می باشد")]
        public int ContractDuration { get; set; }
    }

    public class ContractType
    {
        public SelectedTypeObject SelectedType { get; set; }

        /// <summary>
        /// مکان تحویل
        /// </summary>
        public SelectedContractObject SelectedDelivery { get; set; }


        public SelectedCurrencyObject SelectedCurrency { get; set; }

        public decimal Tax { get; set; }
    }

    public class SelectedTypeObject
    {
        public PContractType Value { get; set; }
    }
    public class SelectedContractObject
    {
        public POIncoterms Value { get; set; }
    }
    public class SelectedCurrencyObject
    {
        public CurrencyType Value { get; set; }
    }
    
}