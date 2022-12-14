using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PO.POSupplierDocuemnt
{
    public class AddPOSupplierDocumentDto
    {
        public string DocumentTitle { get; set; }
        public string DocumentCode { get; set; }
        public int ProdcutId { get; set; }
        public List<AddAttachmentDto> Attachments { get; set; }
    }
}
