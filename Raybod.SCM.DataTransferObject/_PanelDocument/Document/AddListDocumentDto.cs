using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.DataTransferObject.Document;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class AddListDocumentDto : BaseDocumentDto
    {
        public List<int> ProductIds { get; set; }
        public AreaReadDTO Area { get; set; }
    }
}
