
using Raybod.SCM.DataTransferObject.Supplier;
using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class EditPrContractInfoDto
    {
        public ContractTimeTable ContractTimeTable { get; set; }
        public int ProductGroupId { get; set; }
        public long PRContractId { get; set; }
        public string ProductGroupTitle { get; set; }

        public string PRContractCode { get; set; }

        public PRContractStatus PRContractStatus { get; set; }
        public SupplierMiniInfoDto Supplier { get; set; }
        public decimal TotalAmount { get; set; }

        public decimal FinalTotalAmount { get; set; }
        public decimal TaxAmount { get; set; }

        public string FinalTotalAmountInLetters { get; set; }
        public int SupplierId { get; set; }
        public ContractForEditType ContractType { get; set; }
        public PaymentSteps PaymentSteps { get; set; }
        public UserAuditLogDto UserAudit { get; set; }
        public List<ListPRContractSubjectToEditInfoDto> PRContractSubjects { get; set; }
        public List<EditTermsOfPaymentDto> TermsOfPayments { get; set; }
        public List<BasePRContractAttachmentDto> Attachments { get; set; }
        public EditPrContractInfoDto()
        {
            PRContractSubjects = new List<ListPRContractSubjectToEditInfoDto>();
            TermsOfPayments = new List<EditTermsOfPaymentDto>();
            Attachments = new List<BasePRContractAttachmentDto>();
        }


    }
   

    

    
    public class ContractForEditType
    {
        public PContractType SelectedType { get; set; }

        /// <summary>
        /// مکان تحویل
        /// </summary>
        public POIncoterms SelectedDelivery { get; set; }


        public CurrencyType SelectedCurrency { get; set; }

        public decimal Tax { get; set; }
    }

}