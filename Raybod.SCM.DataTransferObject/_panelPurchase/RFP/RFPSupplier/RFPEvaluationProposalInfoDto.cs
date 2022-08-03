using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class RFPEvaluationProposalInfoDto
    {
        [MaxLength(800)]
        public string EvaluationNote { get; set; }

        public Nullable<decimal> EvaluationScore { get; set; }

        public RFPInqueryType InqueryType { get; set; }

        public List<ListSupplierProposalInqueryDto> SupplierProposals { get; set; }

        public RFPEvaluationProposalInfoDto()
        {
            SupplierProposals = new List<ListSupplierProposalInqueryDto>();
        }
    }
}
