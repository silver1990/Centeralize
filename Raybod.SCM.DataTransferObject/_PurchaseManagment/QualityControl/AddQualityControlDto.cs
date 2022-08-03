using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.QualityControl
{
    public class AddQualityControlDto : BaseQualityControlDto
    {
        public List<AddAttachmentDto> Attachments { get; set; }
    }
}