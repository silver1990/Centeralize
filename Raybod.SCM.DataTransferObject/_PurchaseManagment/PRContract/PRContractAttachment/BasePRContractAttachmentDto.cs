using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.PRContract
{
    public class BasePRContractAttachmentDto : BasePAttachmentDto
    {
        public long PRContractId { get; set; }
    }
    public class BasePRContractConfirmationAttachmentDto : BasePAttachmentDto
    {
        public long? PrContractConfirmWorkFlowId { get; set; }
    }
}