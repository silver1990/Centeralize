using Raybod.SCM.DataTransferObject.PRContract;

namespace Raybod.SCM.DataTransferObject.QualityControl
{
    public class BaseQualityControlAttachmentDto : BasePAttachmentDto
    {
        public long QualityControlId { get; set; }
    }
}