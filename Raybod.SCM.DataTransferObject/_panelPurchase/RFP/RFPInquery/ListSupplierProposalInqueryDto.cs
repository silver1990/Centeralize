using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class ListSupplierProposalInqueryDto : BaseRFPInqueryDto
    {
        public long Id { get; set; }

        public long RFPInqueryId { get; set; }

        public List<RFPInqueryAttachmentDto> InqueryAttachments { get; set; }

        public long RFPSupplierId { get; set; }

        public bool IsEvaluated { get; set; }
        public bool IsAnswered { get; set; }

        public string PoroposalDescription { get; set; }

        public  Nullable<decimal> EvaluationScore { get; set; }

        public List<RFPInqueryAttachmentDto> PoroposalAttachments { get; set; }
        public RFPInqueryType RFPInqueryType { get; set; }
        public ListSupplierProposalInqueryDto()
        {
            InqueryAttachments = new List<RFPInqueryAttachmentDto>();
            PoroposalAttachments = new List<RFPInqueryAttachmentDto>();
        }
    }
}
