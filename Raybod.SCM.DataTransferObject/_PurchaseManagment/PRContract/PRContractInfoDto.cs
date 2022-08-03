using System.Collections.Generic;
using Raybod.SCM.DataTransferObject.RFP;
using Raybod.SCM.DataTransferObject.Supplier;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class PRContractInfoDto : BasePRContractDto
    {
        public PrContractConfirmationWorkflowDto WorkFlow { get; set; }
        public long PRContractId { get; set; }

        public string ProductGroupTitle { get; set; }

        public string PRContractCode { get; set; }

        public PRContractStatus PRContractStatus { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal FinalTotalAmount { get; set; }
        public decimal TaxAmount { get; set; }

        public string FinalTotalAmountInLetters { get; set; }

        public BaseSupplierDto Supplier { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public List<EditTermsOfPaymentDto> TermsOfPayments { get; set; }
        public List<ListPRContractSubjectDto> PRContractSubjects { get; set; }
        public List<BasePRContractAttachmentDto> Attachments { get; set; }

        public PRContractInfoDto()
        {
            Supplier = new BaseSupplierDto();
            UserAudit = new UserAuditLogDto();
            TermsOfPayments = new List<EditTermsOfPaymentDto>();
            PRContractSubjects = new List<ListPRContractSubjectDto>();
            Attachments = new List<BasePRContractAttachmentDto>();
        }
    }
}