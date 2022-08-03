using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Document
{
    public class AddDocumentDto : BaseDocumentDto
    {
        public List<int> ProductIds { get; set; }
    }
}
