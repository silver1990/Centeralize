using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.Invoice
{
    public class AddInvoiceDto
    {


        public WaitingForInvoiceType WaitingForInvoiceType { get; set; }

        [MaxLength(800)]
        public string Note { get; set; }

        public List<AddAttachmentDto> Attachments { get; set; }
    }
}
