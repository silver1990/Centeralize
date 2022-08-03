using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PO.POInspection
{
    public class AddPOInspectionResultDto
    {
        public string ResultNote { get; set; }
        public InspectionResult Result { get; set; }
        public List<AddAttachmentDto> Attachments { get; set; }
    }
}
