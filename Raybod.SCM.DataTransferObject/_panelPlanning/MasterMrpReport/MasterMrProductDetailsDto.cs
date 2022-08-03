using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.MasterMrpReport
{
    public class MasterMrProductDetailsDto : MasterMrProductListDto
    {
        public EngineeringDocumentStatus DocumentStatus { get; set; }
    }
}
