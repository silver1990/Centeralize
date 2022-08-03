using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class BaseTermsOfPaymentDto : AddTermsOfPaymentDto
    {
        public long PRContractId { get; set; }
        
    }
}