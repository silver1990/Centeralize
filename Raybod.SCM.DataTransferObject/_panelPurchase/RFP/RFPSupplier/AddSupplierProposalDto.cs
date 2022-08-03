using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class AddSupplierProposalDto
    {
        public long Id { get; set; }

        public long RFPInqueryId { get; set; }

        public string PoroposalDescription { get; set; }

        public List<RFPInqueryAttachmentDto> PoroposalAttachments { get; set; }

    }
}
