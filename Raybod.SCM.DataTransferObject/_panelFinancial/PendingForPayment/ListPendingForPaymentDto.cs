using Raybod.SCM.DataTransferObject.Supplier;
using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.PendingForPayment
{
    public class ListPendingForPaymentDto : BasePendingForPaymentDto
    {
        
        public string POCode { get; set; }

        public string InvoiceNumber { get; set; }

        public string SupplierName { get; set; }
        
        public string SupplierCode { get; set; }

        public string PRContractCode { get; set; }

        public TermsOfPaymentStep PaymentStep { get; set; }
        public CurrencyType CurrencyType { get; set; }

        //public SupplierInformationDto SupplierInfo { get; set; }

        public UserAuditLogDto UserAudit { get; set; }

        public ListPendingForPaymentDto()
        {
            //SupplierInfo = new SupplierInformationDto(); 
            UserAudit = new UserAuditLogDto();
        }

    }
    
}
